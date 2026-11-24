using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

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
        return new GeometryFacts(placement, frame, cut, null,
            cut is null ? "Не подтверждён прямоугольный сквозной вырез хоста." : null);
    }

    // Положительная ось семейства отражается самим Transform; положение тела с выпусками не влияет на центр.
    private AlignmentPlanes ReadAnchor(IGeometryReadContext geometry, FamilyInstance instance,
        InstancePlacement placement) {
        using var transform = instance.GetTransform();
        if(instance.Location is not LocationPoint || !transform.IsConformal
            || Math.Abs(transform.BasisZ.Z - 1) > DirectionEpsilon
            || Math.Abs(transform.BasisX.Z) > DirectionEpsilon
            || Math.Abs(transform.BasisY.Z) > DirectionEpsilon) {
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
            ? new AlignmentPlanes(placement.Origin, AsPoint(transform.BasisX.Normalize()),
                AsPoint(transform.BasisY.Normalize()), AsPoint(transform.BasisZ.Normalize()))
            : null;
    }

    // Сопоставляет грани откосов с породившей их вставкой, в том числе у выреза до низа стены.
    private OpeningCut ReadCut(IGeometryReadContext geometry, FamilyInstance instance, Wall wall, WallFrame frame,
        double? widthMm, double? heightMm) {
        var faces = geometry.GetRevealFaces(wall)?[instance.Id].ToList();
        if(faces is null || faces.Count < 3 || faces.Count > 4) {
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
        if(left is null || right is null || head is null || !Near(left.XMin, head.XMin)
            || !Near(right.XMax, head.XMax) || !Near(left.ZMax, head.ZMax) || !Near(right.ZMax, head.ZMax)
            || !Near(left.ZMin, right.ZMin) || (sill is not null && (!Near(sill.ZMin, left.ZMin)
                || !Near(sill.XMin, head.XMin) || !Near(sill.XMax, head.XMax)))) {
            return null;
        }
        double width = head.XMax - head.XMin;
        double height = head.ZMax - left.ZMin;
        double x = (head.XMin + head.XMax) / 2;
        double z = (head.ZMax + left.ZMin) / 2;
        var front = AsXyz(frame.ToWorld(x, frame.Thickness / 2, z));
        var back = AsXyz(frame.ToWorld(x, -frame.Thickness / 2, z));
        if(geometry.Intersect(wall, back, front)?.Count != 0) {
            return null;
        }
        var top = frame.ToWorld(x, 0, head.ZMax);
        var level = geometry.Document.GetElement(instance.LevelId) as Level;
        var match = MatchHeight(heightMm, height, top.Z - level?.ProjectElevation);
        var target = new AlignmentPlanes(top, frame.Along, frame.Normal, AsPoint(XYZ.BasisZ));
        return new OpeningCut(frame.ToWorld(x, 0, z), target, width, height, MatchesLength(widthMm, width), match);
    }

    // Признаёт только плоский прямоугольный откос через всю толщину без дополнительных петель.
    private static Reveal ReadReveal(Face face, WallFrame frame, XYZ xAxis) {
        if(face is not PlanarFace plane || face.EdgeLoops.Size != 1) {
            return null;
        }
        var normal = plane.FaceNormal;
        RevealSide side;
        if(normal.CrossProduct(XYZ.BasisZ).GetLength() <= DirectionEpsilon) {
            side = normal.Z < 0 ? RevealSide.Head : RevealSide.Sill;
        } else if(normal.CrossProduct(xAxis).GetLength() <= DirectionEpsilon) {
            side = normal.DotProduct(xAxis) > 0 ? RevealSide.Left : RevealSide.Right;
        } else {
            return null;
        }
        var edges = face.EdgeLoops.get_Item(0);
        if(edges.Size != 4) {
            return null;
        }
        var points = new List<GeometryPoint>();
        foreach(Edge edge in edges) {
            using var curve = edge.AsCurveFollowingFace(face);
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
        // Каждая из четырёх комбинаций координат должна принадлежать реальному углу грани.
        foreach(double span in new[] { min, max }) {
            foreach(double depth in new[] { -frame.Thickness / 2, frame.Thickness / 2 }) {
                if(!points.Any(p => Near(horizontal ? p.X : p.Z, span) && Near(p.Y, depth))) {
                    return null;
                }
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
        return new InstancePlacement(AsPoint(transform.Origin), AsPoint(hand.Normalize()),
            AsPoint(facing.Normalize()), instance.Mirrored, instance.HandFlipped, instance.FacingFlipped);
    }
}
