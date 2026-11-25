using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using static RevitLintelsManager.Models.RevitElementRead;
using static RevitLintelsManager.Models.RevitGeometry;

namespace RevitLintelsManager.Models;

/// <summary>Определяет фактический вырез проёма и плоскости привязки перемычки.</summary>
internal sealed class PlacementGeometryReader : IPlacementGeometryReader {
    private enum RevealSide { Left, Right, Head, Sill }

    private sealed class Reveal(RevealSide side, double xmin, double xmax, double zmin, double zmax) {
        public RevealSide Side { get; } = side;
        public double XMin { get; } = xmin;
        public double XMax { get; } = xmax;
        public double ZMin { get; } = zmin;
        public double ZMax { get; } = zmax;
    }

    /// <summary>Возвращает привязку экземпляра, если три центральные плоскости задают начало семейства.</summary>
    public GeometryFacts ReadLintelGeometry(IGeometryReadContext geometry, FamilyInstance instance) {
        var placement = ReadOptional(() => GetPlacement(instance));
        var frame = geometry.GetWallFrame(ReadOptional(() => instance.Host as Wall));
        var anchor = placement is null ? null : ReadOptional(() => ReadAnchor(geometry, instance, placement));
        return new GeometryFacts(placement, frame, null, anchor,
            anchor is null ? "Не подтверждена привязка к трём центральным плоскостям семейства." : null);
    }

    /// <summary>Читает контур фактического выреза и независимо сверяет размеры, заданные в настройках.</summary>
    public GeometryFacts ReadOpeningGeometry(IGeometryReadContext geometry, FamilyInstance instance,
        double? widthMm, double? heightMm) {
        var placement = ReadOptional(() => GetPlacement(instance));
        var wall = ReadOptional(() => instance.Host as Wall);
        var frame = geometry.GetWallFrame(wall);
        if(frame is null) {
            return new GeometryFacts(placement, null, null, null, "Не определены боковые плоскости прямой стены.");
        }
        var cut = ReadOptional(() => ReadCut(geometry, instance, wall, frame, widthMm, heightMm));
        if(cut is not null) {
            return new GeometryFacts(placement, frame, cut, null, null);
        }
        var openTop = ReadOptional(() => ReadOpenTop(geometry, instance, wall, frame, widthMm, heightMm));
        var searchTop = openTop.HasValue ? null
            : ReadOptional(() => ReadSearchTop(geometry, instance, wall, frame, widthMm, heightMm));
        bool hasConflictingCut = !openTop.HasValue
            && ReadOptional(() => HasConflictingCut(geometry, instance, wall, frame));
        return new GeometryFacts(placement, frame, "Не подтверждён замкнутый сверху вырез стены-хоста.",
            openTop, searchTop, hasConflictingCut);
    }

    /// <inheritdoc />
    public (Wall Wall, GeometryFacts Geometry) ReadAdjacentOpeningGeometry(IGeometryReadContext geometry,
        FamilyInstance instance, double? widthMm, double? heightMm, IEnumerable<Wall> candidates) {
        var host = ReadOptional(() => instance.Host as Wall);
        var hostFaces = host is null ? null : ReadOptional(() => geometry.GetRevealFaces(host));
        if(hostFaces is null || hostFaces[instance.Id].Any()) {
            return default;
        }

        var placement = ReadOptional(() => GetPlacement(instance));
        (Wall Wall, GeometryFacts Geometry) found = default;
        foreach(var wall in candidates.Where(wall => wall.Id != host.Id)) {
            var frame = geometry.GetWallFrame(wall);
            if(frame is null) {
                continue;
            }
            var cut = ReadOptional(() => ReadCut(geometry, instance, wall, frame, widthMm, heightMm));
            if(cut is null) {
                continue;
            }
            if(found.Wall is not null) {
                return default;
            }
            found = (wall, new GeometryFacts(placement, frame, cut, null, null));
        }
        return found;
    }

    // Проверяет разрыв или укорочение горизонтальной грани по откосам другого выреза этой стены.
    private static bool HasConflictingCut(IGeometryReadContext geometry, FamilyInstance instance, Wall wall,
        WallFrame frame) {
        var faces = geometry.GetRevealFaces(wall);
        if(faces is null) {
            return false;
        }
        var along = AsXyz(frame.Along);
        var sections = faces[instance.Id].OfType<PlanarFace>()
            .Where(face => face.FaceNormal.CrossProduct(XYZ.BasisZ).GetLength() <= DirectionEpsilon)
            .SelectMany(face => face.EdgeLoops.Cast<EdgeArray>().Select(edges => ReadReveal(face, edges, frame, along)))
            .OfType<Reveal>().ToList();
        if(sections.Count < 2) {
            return false;
        }
        foreach(var entry in faces.Where(entry => entry.Key != instance.Id)) {
            var other = geometry.Document.GetElement(entry.Key);
            if(other is not Opening && (other is not FamilyInstance insert
                || insert.Category?.GetBuiltInCategory() is not (BuiltInCategory.OST_Doors or BuiltInCategory.OST_Windows))) {
                continue;
            }
            var jambs = entry.Select(face => ReadReveal(face, frame, along)).OfType<Reveal>().ToList();
            if(jambs.Where(jamb => jamb.Side == RevealSide.Left).Any(left =>
                jambs.Where(jamb => jamb.Side == RevealSide.Right && jamb.XMax > left.XMin + LengthEpsilon)
                    .Any(right => BreaksReveal(sections, left, right, RevealSide.Head)
                        || BreaksReveal(sections, left, right, RevealSide.Sill)
                        || TrimsReveal(sections, left, right)))) {
                return true;
            }
        }
        return false;
    }

    // Связывает кромки двух фрагментов грани с началом или концом соседних откосов.
    private static bool BreaksReveal(IReadOnlyList<Reveal> sections, Reveal left, Reveal right, RevealSide side) {
        double height = side == RevealSide.Head ? left.ZMin : left.ZMax;
        double rightHeight = side == RevealSide.Head ? right.ZMin : right.ZMax;
        return Near(height, rightHeight)
            && sections.Any(section => section.Side == side && Near(section.ZMax, height)
                && Near(section.XMax, left.XMin))
            && sections.Any(section => section.Side == side && Near(section.ZMax, height)
                && Near(section.XMin, right.XMax));
    }

    // Подтверждает укороченную грань по полной противоположной грани и двум откосам соседнего выреза.
    private static bool TrimsReveal(IReadOnlyList<Reveal> sections, Reveal left, Reveal right) {
        foreach(var section in sections) {
            double height = section.ZMax;
            bool isHead = section.Side == RevealSide.Head;
            double leftContact = isHead ? left.ZMin : left.ZMax;
            double rightContact = isHead ? right.ZMin : right.ZMax;
            bool touchesLeft = Near(section.XMax, left.XMin) && Near(leftContact, height)
                && CoversHeight(right, height);
            bool touchesRight = Near(section.XMin, right.XMax) && Near(rightContact, height)
                && CoversHeight(left, height);
            if((touchesLeft || touchesRight) && sections.Any(other => other.Side != section.Side
                && (isHead ? other.ZMax < height - LengthEpsilon : other.ZMax > height + LengthEpsilon)
                && ((touchesLeft && Near(other.XMin, section.XMin) && Near(other.XMax, right.XMax))
                    || (touchesRight && Near(other.XMin, left.XMin) && Near(other.XMax, section.XMax))))) {
                return true;
            }
        }
        return false;
    }

    // Проверяет, что откос соседнего выреза существует на отметке укороченной грани.
    private static bool CoversHeight(Reveal jamb, double height) {
        return jamb.ZMin <= height + LengthEpsilon && jamb.ZMax >= height - LengthEpsilon;
    }

    // Локализует верх для исключения далёких кандидатов при неполном контуре выреза.
    private GeometryPoint? ReadSearchTop(IGeometryReadContext geometry, FamilyInstance instance, Wall wall,
        WallFrame frame, double? widthMm, double? heightMm) {
        if(!widthMm.HasValue || !heightMm.HasValue || geometry.Document.GetElement(instance.LevelId) is not Level level) {
            return null;
        }
        var faces = geometry.GetRevealFaces(wall)?[instance.Id].ToList();
        if(faces is null || faces.Count == 0) {
            return null;
        }
        var along = AsXyz(frame.Along);
        double top = level.ProjectElevation - frame.Origin.Z
            + UnitUtils.ConvertToInternalUnits(heightMm.Value, UnitTypeId.Millimeters);
        var reveals = faces.Select(face => ReadReveal(face, frame, along)).Where(reveal => reveal is not null).ToList();
        var heads = reveals.Where(reveal => reveal.Side == RevealSide.Head).ToList();
        if(heads.Count > 0) {
            var head = heads[0];
            return heads.Count == 1 && Near(head.ZMax, top) && MatchesLength(widthMm, head.XMax - head.XMin) == true
                ? frame.ToWorld((head.XMin + head.XMax) / 2, 0, head.ZMax) : null;
        }
        var sills = reveals.Where(reveal => reveal.Side == RevealSide.Sill).ToList();
        if(sills.Count != 1) {
            return null;
        }
        var sill = sills[0];
        if(top <= sill.ZMax + LengthEpsilon || MatchesLength(widthMm, sill.XMax - sill.XMin) != true
            || !faces.Any(face => HasJambTop(face, frame, along, sill, top))) {
            return null;
        }

        double x = (sill.XMin + sill.XMax) / 2;
        double middle = (sill.ZMax + top) / 2;
        var back = AsXyz(frame.ToWorld(x, -frame.Thickness / 2, middle));
        var front = AsXyz(frame.ToWorld(x, frame.Thickness / 2, middle));
        return geometry.Intersect(wall, back, front)?.Count == 0 ? frame.ToWorld(x, 0, top) : null;
    }

    // Сверяет отметку с верхней кромкой одного из откосов, в том числе с разорванным контуром.
    private static bool HasJambTop(Face face, WallFrame frame, XYZ along, Reveal sill, double top) {
        if(face is not PlanarFace plane || plane.FaceNormal.CrossProduct(along).GetLength() > DirectionEpsilon) {
            return false;
        }
        double x = frame.ToLocal(AsPoint(plane.Origin)).X;
        double side = plane.FaceNormal.DotProduct(along) > 0 ? sill.XMin : sill.XMax;
        if(!Near(x, side)) {
            return false;
        }
        var elevations = face.EdgeLoops.Cast<EdgeArray>().SelectMany(loop => loop.Cast<Edge>()).Select(edge => {
            using var curve = edge.AsCurveFollowingFace(face);
            return frame.ToLocal(AsPoint(curve.GetEndPoint(0))).Z;
        });
        return Near(elevations.DefaultIfEmpty(double.NaN).Max(), top);
    }

    // Положительная ось семейства отражается самим Transform; положение тела с выпусками не влияет на центр.
    private AlignmentPlanes ReadAnchor(IGeometryReadContext geometry, FamilyInstance instance,
        InstancePlacement placement) {
        var axes = placement.TransformAxes;
        if(instance.Location is not LocationPoint || axes is null
            || Math.Abs(axes.HeightNormal.Z - 1) > DirectionEpsilon
            || Math.Abs(axes.CenterNormal.Z) > DirectionEpsilon
            || Math.Abs(axes.LongitudinalNormal.Z) > DirectionEpsilon) {
            return null;
        }
        var family = instance.Symbol.Family;
        if(geometry.GetOriginPlanes(family) is not { } planes) {
            return null;
        }
        bool Matches(FamilyInstanceReferenceType kind, string name) {
            var references = instance.GetReferences(kind);
            return references?.Count == 1 && instance.GetReferenceName(references[0]) == name;
        }
        return Matches(FamilyInstanceReferenceType.CenterLeftRight, planes.X)
            && Matches(FamilyInstanceReferenceType.CenterFrontBack, planes.Y)
            && Matches(FamilyInstanceReferenceType.CenterElevation, planes.Z)
            ? axes : null;
    }

    // Сопоставляет откосы вставки, включая вырезы до низа или торца прямой стены.
    private OpeningCut ReadCut(IGeometryReadContext geometry, FamilyInstance instance, Wall wall, WallFrame frame,
        double? widthMm, double? heightMm) {
        var faces = geometry.GetRevealFaces(wall)?[instance.Id].ToList();
        if(faces is null || faces.Count < 2 || faces.Count > 4) {
            return null;
        }
        var xAxis = AsXyz(frame.Along);
        var reveals = faces.Select(face => ReadReveal(face, frame, xAxis)).ToList();
        if(reveals.Any(face => face is null)
            || reveals.Select(face => face.Side).Distinct().Count() != faces.Count) {
            return null;
        }
        var left = reveals.Find(face => face.Side == RevealSide.Left);
        var right = reveals.Find(face => face.Side == RevealSide.Right);
        var head = reveals.Find(face => face.Side == RevealSide.Head);
        var sill = reveals.Find(face => face.Side == RevealSide.Sill);
        if(head is null || (left is null && right is null)) {
            return null;
        }
        bool missingJamb = left is null || right is null;
        bool needsLocalEnd = missingJamb && !ReachesWallEnd(wall, head, left is null);
        double bottom = (left ?? right).ZMin;
        if(!MatchesJamb(left, head.XMin, bottom, head.ZMax)
            || !MatchesJamb(right, head.XMax, bottom, head.ZMax)
            || (sill is not null && (!Near(sill.ZMin, bottom) || !Near(sill.XMin, head.XMin)
                || !Near(sill.XMax, head.XMax)))) {
            return null;
        }
        double width = head.XMax - head.XMin;
        double height = head.ZMax - bottom;
        double x = (head.XMin + head.XMax) / 2;
        double z = (head.ZMax + bottom) / 2;
        var front = AsXyz(frame.ToWorld(x, frame.Thickness / 2, z));
        var back = AsXyz(frame.ToWorld(x, -frame.Thickness / 2, z));
        if(geometry.Intersect(wall, back, front)?.Count != 0) {
            return null;
        }
        var top = frame.ToWorld(x, 0, head.ZMax);
        var level = geometry.Document.GetElement(instance.LevelId) as Level;
        var match = MatchHeight(heightMm, height, top.Z - level?.ProjectElevation);
        bool? widthMatches = MatchesLength(widthMm, width);
        if(missingJamb && (sill is null || needsLocalEnd) && (widthMatches != true
            || match is CutHeightMatch.Unavailable or CutHeightMatch.Neither)) {
            return null;
        }
        if(needsLocalEnd && !ReachesEndAboveHead(geometry, wall, frame, head, left is null)) {
            return null;
        }
        var target = new AlignmentPlanes(top, frame.Along, frame.Normal, AsPoint(XYZ.BasisZ));
        return new OpeningCut(frame.ToWorld(x, 0, z), target, width, height, widthMatches, match);
    }

    // У выреза до верха стены низ и откосы задают пролёт и подтверждаются сопоставленными размерами.
    private (GeometryPoint Center, double Width, double JambHeight)? ReadOpenTop(IGeometryReadContext geometry,
        FamilyInstance instance, Wall wall, WallFrame frame, double? widthMm, double? heightMm) {
        var faces = geometry.GetRevealFaces(wall)?[instance.Id].ToList();
        if(faces is null || faces.Count is not (2 or 3)) {
            return null;
        }
        var xAxis = AsXyz(frame.Along);
        var reveals = faces.Select(face => ReadReveal(face, frame, xAxis)).ToList();
        var sill = reveals.Find(reveal => reveal?.Side == RevealSide.Sill);
        var left = reveals.Find(reveal => reveal?.Side == RevealSide.Left);
        var right = reveals.Find(reveal => reveal?.Side == RevealSide.Right);
        var jamb = left ?? right;
        if(sill is null || jamb is null || (faces.Count == 3 && (left is null || right is null))) {
            return null;
        }
        double top = jamb.ZMax;
        if(!MatchesJamb(left, sill.XMin, sill.ZMin, top) || !MatchesJamb(right, sill.XMax, sill.ZMin, top)
            || (left is null && !ReachesWallEnd(wall, sill, true))
            || (right is null && !ReachesWallEnd(wall, sill, false))) {
            return null;
        }
        double width = sill.XMax - sill.XMin;
        double x = (sill.XMin + sill.XMax) / 2;
        var center = frame.ToWorld(x, 0, top);
        var level = geometry.Document.GetElement(instance.LevelId) as Level;
        if(MatchesLength(widthMm, width) != true || level is null
            || MatchesLength(heightMm, center.Z - level.ProjectElevation) != true) {
            return null;
        }
        double middle = (sill.ZMin + top) / 2;
        var front = AsXyz(frame.ToWorld(x, frame.Thickness / 2, middle));
        var back = AsXyz(frame.ToWorld(x, -frame.Thickness / 2, middle));
        return geometry.Intersect(wall, back, front)?.Count == 0 ? (center, width, top - sill.ZMin) : null;
    }

    // Проверяет, что грань выреза доходит до торца стены без бокового откоса.
    private static bool ReachesWallEnd(Wall wall, Reveal span, bool missingLeft) {
        return wall.Location is LocationCurve { Curve: Line axis }
            && (missingLeft ? Near(span.XMin, 0) : Near(span.XMax, axis.Length));
    }

    // Подтверждает укороченный торец профиля, измеряя тело стены над верхней гранью выреза.
    private static bool ReachesEndAboveHead(IGeometryReadContext geometry, Wall wall, WallFrame frame,
        Reveal head, bool missingLeft) {
        var bounds = geometry.GetBounds(wall);
        if(bounds is null) {
            return false;
        }
        double center = (head.XMin + head.XMax) / 2;
        double z = head.ZMax + geometry.Document.Application.ShortCurveTolerance;
        var start = AsXyz(frame.ToWorld(center, 0, z));
        var direction = AsXyz(frame.Along) * (missingLeft ? -1 : 1);
        double reach = AsXyz(bounds.Min).DistanceTo(AsXyz(bounds.Max)) + LengthEpsilon;
        var spans = geometry.Intersect(wall, start, start + direction * reach);
        return spans?.Count == 1 && Near(spans[0].Start, 0)
            && Near(spans[0].End, (head.XMax - head.XMin) / 2);
    }

    // Проверяет плоскость и отметки бокового откоса, если он присутствует.
    private static bool MatchesJamb(Reveal jamb, double edge, double bottom, double top) {
        if(jamb is null) {
            return true;
        }
        double x = jamb.Side == RevealSide.Left ? jamb.XMin : jamb.XMax;
        return Near(x, edge) && Near(jamb.ZMin, bottom) && Near(jamb.ZMax, top);
    }

    // Признаёт только плоский прямоугольный откос через всю толщину без дополнительных петель.
    private static Reveal ReadReveal(Face face, WallFrame frame, XYZ xAxis) {
        if(face is not PlanarFace plane || face.EdgeLoops.Size != 1) {
            return null;
        }
        return ReadReveal(plane, face.EdgeLoops.get_Item(0), frame, xAxis);
    }

    // Проверяет отдельный прямоугольный фрагмент грани через всю толщину стены.
    private static Reveal ReadReveal(PlanarFace plane, EdgeArray edges, WallFrame frame, XYZ xAxis) {
        var normal = plane.FaceNormal;
        RevealSide side;
        if(normal.CrossProduct(XYZ.BasisZ).GetLength() <= DirectionEpsilon) {
            side = normal.Z < 0 ? RevealSide.Head : RevealSide.Sill;
        } else if(normal.CrossProduct(xAxis).GetLength() <= DirectionEpsilon) {
            side = normal.DotProduct(xAxis) > 0 ? RevealSide.Left : RevealSide.Right;
        } else {
            return null;
        }
        if(edges.Size < 4) {
            return null;
        }
        var points = new List<GeometryPoint>();
        foreach(Edge edge in edges) {
            using var curve = edge.AsCurveFollowingFace(plane);
            if(curve is not Line) {
                return null;
            }
            points.Add(frame.ToLocal(AsPoint(curve.GetEndPoint(0))));
        }
        var reveal = new Reveal(side, points.Min(p => p.X), points.Max(p => p.X),
            points.Min(p => p.Z), points.Max(p => p.Z));
        bool horizontal = side is RevealSide.Head or RevealSide.Sill;
        double min = horizontal ? reveal.XMin : reveal.ZMin;
        double max = horizontal ? reveal.XMax : reveal.ZMax;
        double halfThickness = frame.Thickness / 2;
        // Каждая из четырёх комбинаций координат должна принадлежать реальному углу грани.
        foreach(double span in new[] { min, max }) {
            foreach(double depth in new[] { -halfThickness, halfThickness }) {
                if(!points.Any(p => Near(horizontal ? p.X : p.Z, span) && Near(p.Y, depth))) {
                    return null;
                }
            }
        }
        // Revit может разбить прямую сторону несколькими рёбрами, например на отметке уровня.
        for(int i = 0; i < points.Count; i++) {
            var start = points[i];
            var end = points[(i + 1) % points.Count];
            double a = horizontal ? start.X : start.Z;
            double b = horizontal ? end.X : end.Z;
            if(!(Near(a, b) && (Near(a, min) || Near(a, max)))
                && !(Near(start.Y, end.Y) && Near(Math.Abs(start.Y), halfThickness))) {
                return null;
            }
        }
        return max - min > LengthEpsilon ? reveal : null;
    }

    // Допуск используется только при проверке совпадения координат физических граней.
    private static bool Near(double a, double b) {
        return Math.Abs(a - b) <= LengthEpsilon;
    }

    // Числовое совпадение двух трактовок на проёме от уровня не устанавливает семантику параметра.
    private static CutHeightMatch MatchHeight(double? mappedMm, double height, double? aboveLevel) {
        if(!mappedMm.HasValue || !aboveLevel.HasValue) {
            return CutHeightMatch.Unavailable;
        }
        bool size = MatchesLength(mappedMm, height) == true;
        bool head = MatchesLength(mappedMm, aboveLevel.Value) == true;
        if(size) {
            return head ? CutHeightMatch.Both : CutHeightMatch.CutHeight;
        }
        return head ? CutHeightMatch.HeadAboveLevel : CutHeightMatch.Neither;
    }

    // Переводит единственное значение параметра в те же единицы, что измеренная геометрия.
    private static bool? MatchesLength(double? mm, double feet) {
        return mm.HasValue ? Near(UnitUtils.ConvertToInternalUnits(mm.Value, UnitTypeId.Millimeters), feet) : null;
    }

    // Направления Revit уже учитывают поворот и flips; флаги отражения повторно к ним не применяются.
    private static InstancePlacement GetPlacement(FamilyInstance instance) {
        using var transform = instance.GetTransform();
        var hand = instance.HandOrientation;
        var facing = instance.FacingOrientation;
        if(!IsFinitePoint(transform.Origin) || !IsFinitePoint(hand) || !IsFinitePoint(facing)
            || hand.IsZeroLength() || facing.IsZeroLength()) {
            return null;
        }
        var origin = AsPoint(transform.Origin);
        var axes = transform.IsConformal ? new AlignmentPlanes(origin, AsPoint(transform.BasisX.Normalize()),
            AsPoint(transform.BasisY.Normalize()), AsPoint(transform.BasisZ.Normalize())) : null;
        return new InstancePlacement(origin, AsPoint(hand.Normalize()), AsPoint(facing.Normalize()),
            instance.Mirrored, instance.HandFlipped, instance.FacingFlipped, axes);
    }
}
