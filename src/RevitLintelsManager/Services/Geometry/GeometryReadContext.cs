using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using static RevitLintelsManager.Models.RevitElementRead;
using static RevitLintelsManager.Models.RevitGeometry;

namespace RevitLintelsManager.Models;

/// <summary>Владеет геометрией и кэшами исходного документа до конца одного чтения.</summary>
internal sealed class GeometryReadContext(Document document) : IGeometryReadContext {
    private readonly Dictionary<ElementId, ILookup<ElementId, Face>> _reveals = [];
    private readonly Dictionary<ElementId, ((string X, string Y, string Z)? Full, (string Name, double Offset)? Width)>
        _originFamilies = [];

    private readonly Dictionary<ElementId, List<Solid>> _solids = [];
    private readonly Dictionary<ElementId, WallFrame> _wallFrames = [];
    private readonly Dictionary<ElementId, GeometryBounds> _bounds = [];
    private readonly List<GeometryElement> _geometry = [];
    private readonly Options _options = new() { DetailLevel = ViewDetailLevel.Fine, ComputeReferences = false };

    public Document Document => document;

    /// <inheritdoc />
    public void Dispose() {
        _solids.Clear();
        _wallFrames.Clear();
        _bounds.Clear();
        _reveals.Clear();
        _originFamilies.Clear();
        for(int index = _geometry.Count - 1; index >= 0; index--) {
            _geometry[index].Dispose();
        }
        _geometry.Clear();
        _options.Dispose();
    }

    /// <summary>Интервалы материала, пустой список при промахе, null при непригодных данных.</summary>
    public IReadOnlyList<MaterialInterval> Intersect(Element candidate, XYZ start, XYZ end) {
        if(candidate is null || !candidate.IsValidObject || !candidate.Document.Equals(document)
            || !IsFinitePoint(start) || !IsFinitePoint(end)
            || !XYZ.IsWithinLengthLimits(start) || !XYZ.IsWithinLengthLimits(end)
            || start.DistanceTo(end) <= document.Application.ShortCurveTolerance) {
            return null;
        }
        return ReadOptional(() => IntersectCandidate(candidate, start, end));
    }

    /// <summary>Конец вертикального отрезка по границам переданных кандидатов в той же XY-колонке.</summary>
    public GeometryPoint? GetVerticalEnd(IReadOnlyList<Element> candidates, XYZ start, bool upwards) {
        if(candidates is null || !IsFinitePoint(start) || !XYZ.IsWithinLengthLimits(start)) {
            return null;
        }
        return ReadOptional<GeometryPoint?>(() => ReadVerticalEnd(candidates, start, upwards));
    }

    // Неизвестные границы не позволяют доказать конечный предел; при пустой колонке отрезок не нужен.
    private GeometryPoint? ReadVerticalEnd(IReadOnlyList<Element> candidates, XYZ start, bool upwards) {
        double end = start.Z;
        foreach(var candidate in candidates) {
            if(candidate is null || !candidate.IsValidObject || !document.Equals(candidate.Document)) {
                return null;
            }
            foreach(var owner in GetGeometryOwners(candidate)) {
                var box = GetBounds(owner);
                if(box is null) {
                    return null;
                }
                var atMin = new GeometryPoint(start.X, start.Y, box.Min.Z);
                var atMax = new GeometryPoint(start.X, start.Y, box.Max.Z);
                if(box.Overlaps(atMin, atMax, LengthEpsilon)) {
                    end = upwards ? Math.Max(end, box.Max.Z) : Math.Min(end, box.Min.Z);
                }
            }
        }
        end += upwards ? LengthEpsilon : -LengthEpsilon;
        var target = new XYZ(start.X, start.Y, end);
        bool valid = Math.Abs(end - start.Z) > document.Application.ShortCurveTolerance;
        return valid && XYZ.IsWithinLengthLimits(target) ? AsPoint(target) : null;
    }

    // Отсеивает кандидата до чтения тел; отсутствие надёжного bbox оставляет кандидата в проверке.
    private IReadOnlyList<MaterialInterval> IntersectCandidate(Element candidate, XYZ start, XYZ end) {
        var owners = GetGeometryOwners(candidate).ToList();
        var a = AsPoint(start);
        var b = AsPoint(end);
        if(owners.Contains(null)) {
            return null;
        }
        if(owners.All(owner => GetBounds(owner)?.Overlaps(a, b, LengthEpsilon) == false)) {
            return Array.Empty<MaterialInterval>();
        }
        var solids = GetSolids(candidate, owners);
        if(solids is null) {
            return null;
        }
        return IntersectSolids(solids, start, end);
    }

    // Получает интервалы от начала направленного отрезка для уже отобранных замкнутых тел.
    private static IReadOnlyList<MaterialInterval> IntersectSolids(List<Solid> solids, XYZ start, XYZ end) {
        using var line = Line.CreateBound(start, end);
        using var options = new SolidCurveIntersectionOptions {
            ResultType = SolidCurveIntersectionMode.CurveSegmentsInside
        };
        var material = new List<MaterialInterval>();
        foreach(var solid in solids) {
            using var result = solid.IntersectWithCurve(line, options);
            CollectInterior(solid, line, result, material);
        }
        return MaterialInterval.Union(material);
    }

    // Возвращает только полную выборку замкнутых тел; пустые служебные оболочки Revit пропускает.
    private List<Solid> GetSolids(Element element, IReadOnlyList<Element> owners) {
        if(!_solids.TryGetValue(element.Id, out var solids)) {
            solids = new List<Solid>();
            if(!owners.All(owner => CollectSolids(owner.get_Geometry(_options), solids)) || solids.Count == 0) {
                solids = null;
            }
            _solids.Add(element.Id, solids);
        }
        return solids;
    }

    /// <inheritdoc />
    public IEnumerable<Element> GetGeometryOwners(Element root) {
        var visited = new HashSet<ElementId>();
        var remaining = new Stack<Element>();
        remaining.Push(root);
        while(remaining.Count > 0) {
            var owner = remaining.Pop();
            if(owner is null) {
                yield return null;
                yield break;
            }
            if(!visited.Add(owner.Id)) {
                continue;
            }
            yield return owner;
            if(owner is FamilyInstance family) {
                foreach(var id in family.GetSubComponentIds()) {
                    remaining.Push(document.GetElement(id));
                }
            }
        }
    }

    // Экземпляр уже возвращает геометрию в системе владельца, включая положение вложенных экземпляров.
    private bool CollectSolids(GeometryElement geometry, ICollection<Solid> solids) {
        if(geometry is null) {
            return true;
        }
        _geometry.Add(geometry);
        foreach(var item in geometry) {
            if(item is GeometryInstance instance && !CollectSolids(instance.GetInstanceGeometry(), solids)) {
                return false;
            }
            if(item is Mesh) {
                return false;
            }
            if(item is Solid solid && solid.Faces.Size > 0) {
                if(solid.Edges.Size == 0 || !(solid.Volume > 0)
                    || solid.Edges.Cast<Edge>().Any(edge => edge.GetFace(0) is null || edge.GetFace(1) is null)) {
                    return false;
                }
                solids.Add(solid);
            }
        }
        return true;
    }

    // Разбивает отрезок по границам фактического тела. Ядро может отбросить короткий вход или воздушный разрыв.
    private static void CollectInterior(Solid solid, Line line, SolidCurveIntersection result,
        List<MaterialInterval> material) {
        var start = line.GetEndPoint(0);
        var direction = line.Direction;
        var breaks = new SortedSet<double> { 0, line.Length };
        var entries = new SortedSet<double>();
        var exits = new SortedSet<double>();
        foreach(Face face in solid.Faces) {
            face.Intersect(line, out var points);
            using(points) {
                if(points is null) {
                    continue;
                }
                foreach(IntersectionResult point in points) {
                    double offset = (point.XYZPoint - start).DotProduct(direction);
                    breaks.Add(offset);
                    double sign = face.ComputeNormal(point.UVPoint).DotProduct(direction);
                    if(Math.Abs(sign) > DirectionEpsilon && IsInsideFaceTrim(face, point.XYZPoint)) {
                        (sign < 0 ? entries : exits).Add(offset);
                    }
                }
            }
        }
        if(result.SegmentCount == 0 && entries.Count == 0 && exits.Count == 0) {
            return;
        }
        foreach(Edge edge in solid.Edges) {
            using var curve = edge.AsCurve();
            curve.Intersect(line, out var hits);
            AddBreaks(hits, start, direction, breaks);
            if(curve is Line) {
                for(int i = 0; i < 2; i++) {
                    var offset = curve.GetEndPoint(i) - start;
                    if(offset.CrossProduct(direction).GetLength() <= LengthEpsilon) {
                        breaks.Add(offset.DotProduct(direction));
                    }
                }
            }
        }
        var inside = new List<MaterialInterval>();
        for(int i = 0; i < result.SegmentCount; i++) {
            using var extent = result.GetCurveSegmentExtents(i);
            double a = Math.Max(0, extent.StartParameter - line.GetEndParameter(0));
            double b = Math.Min(line.Length, extent.EndParameter - line.GetEndParameter(0));
            breaks.Add(a);
            breaks.Add(b);
            inside.Add(new MaterialInterval(a, b));
        }

        double left = 0;
        foreach(double right in breaks.GetViewBetween(0, line.Length)) {
            if(right - left > LengthEpsilon) {
                double mid = (left + right) / 2;
                bool outside = HasCrossing(exits, left) || HasCrossing(entries, right);
                bool fromInside = inside.Any(interval => interval.Start <= mid && mid <= interval.End);
                bool enter = HasCrossing(entries, left);
                bool leave = HasCrossing(exits, right);
                bool crossesInterior = (enter && leave) || (left == 0 && leave) || (enter && right == line.Length);
                if(!outside && (fromInside || crossesInterior) && IsInterior(solid, start + direction * mid)) {
                    material.Add(new MaterialInterval(left, right));
                }
            }
            left = right;
        }
    }

    // Сопоставляет практически совпавшие параметры, вычисленные кривой и гранью раздельно.
    private static bool HasCrossing(SortedSet<double> points, double offset) {
        return points.GetViewBetween(offset - LengthEpsilon, offset + LengthEpsilon).Count > 0;
    }

    // Нормаль задаёт вход/выход только строго внутри обрезанной грани; на ребре знак может быть ложным.
    private static bool IsInsideFaceTrim(Face face, XYZ point) {
        foreach(EdgeArray loop in face.EdgeLoops) {
            foreach(Edge edge in loop) {
                using var curve = edge.AsCurve();
                double? distance = ReadOptional<double?>(() => curve.Distance(point));
                if(!distance.HasValue || distance.Value <= LengthEpsilon) {
                    return false;
                }
            }
        }
        return true;
    }

    // Добавляет дискретные точки пересечения, возвращённые ядром Revit.
    private static void AddBreaks(IntersectionResultArray hits, XYZ start, XYZ direction,
        SortedSet<double> breaks) {
        using(hits) {
            if(hits is null) {
                return;
            }
            foreach(IntersectionResult hit in hits) {
                if(hit.XYZPoint is XYZ point) {
                    breaks.Add((point - start).DotProduct(direction));
                }
            }
        }
    }

    // CurveSegmentsInside включает линии на поверхности, поэтому проверяет расстояние до обрезанных граней.
    private static bool IsInterior(Solid solid, XYZ point) {
        foreach(Face face in solid.Faces) {
            using var projection = face.Project(point);
            if(projection is not null && projection.Distance <= LengthEpsilon) {
                return false;
            }
        }
        return true;
    }

    // Читает рамку каждого владельца один раз за поколение; отсутствие рамки допускает точную проверку тела.
    public GeometryBounds GetBounds(Element owner) {
        if(owner is null) {
            return null;
        }
        if(!_bounds.TryGetValue(owner.Id, out var value)) {
            using var bounds = ReadOptional(() => owner.get_BoundingBox(null));
            value = ReadOptional(() => GetWorldBounds(bounds));
            _bounds.Add(owner.Id, value);
        }
        return value;
    }

    // Переводит восемь углов локального BoundingBoxXYZ в охватывающий мировой AABB.
    private static GeometryBounds GetWorldBounds(BoundingBoxXYZ bounds) {
        if(bounds is null || !bounds.IsSet || !bounds.Enabled || !HasValidBounds(bounds.Min, bounds.Max)) {
            return null;
        }
        var corners = GetWorldCorners(bounds).ToArray();
        if(!corners.All(IsFinitePoint)) {
            return null;
        }
        return new GeometryBounds(
            new GeometryPoint(corners.Min(p => p.X), corners.Min(p => p.Y), corners.Min(p => p.Z)),
            new GeometryPoint(corners.Max(p => p.X), corners.Max(p => p.Y), corners.Max(p => p.Z)));
    }

    // В охватывающую рамку входят также разделяемые вложенные экземпляры, участвующие в Intersect.
    public GeometryBounds GetOwnerEnvelope(Element element) {
        if(element is not FamilyInstance) {
            return GetBounds(element);
        }
        var bounds = GetGeometryOwners(element).Select(GetBounds).ToList();
        if(bounds.Any(b => b is null)) {
            return null;
        }
        if(bounds.Count == 1) {
            return bounds[0];
        }
        return new GeometryBounds(
            new GeometryPoint(bounds.Min(b => b.Min.X), bounds.Min(b => b.Min.Y), bounds.Min(b => b.Min.Z)),
            new GeometryPoint(bounds.Max(b => b.Max.X), bounds.Max(b => b.Max.Y), bounds.Max(b => b.Max.Z)));
    }

    /// <inheritdoc />
    public (string X, string Y, string Z)? GetOriginPlanes(Family family) {
        return GetOriginFacts(family).Full;
    }

    /// <inheritdoc />
    public string GetWidthOriginPlane(Family family) {
        return GetOriginFacts(family).Width?.Name;
    }

    /// <inheritdoc />
    public double? GetWidthOriginOffset(Family family) {
        return GetOriginFacts(family).Width?.Offset;
    }

    // Семейство открывается не более одного раза за обновление, включая недоступную привязку.
    private ((string X, string Y, string Z)? Full, (string Name, double Offset)? Width) GetOriginFacts(Family family) {
        if(!_originFamilies.TryGetValue(family.Id, out var origin)) {
            origin = ReadOptional(() => ReadOriginPlanes(family));
            _originFamilies.Add(family.Id, origin);
        }
        return origin;
    }

    // Читает только плоскости Defines Origin из загруженного определения семейства один раз за поколение.
    private ((string X, string Y, string Z)? Full, (string Name, double Offset)? Width) ReadOriginPlanes(
        Family family) {
        if(!family.IsEditable || family.IsInPlace) {
            return default;
        }
        var definition = document.EditFamily(family);
        try {
            return ReadFamilyOriginFacts(definition);
        } finally {
            definition.Close(false);
        }
    }

    // Проверяет, что три плоскости начала лежат на локальных координатных осях.
    internal static (string X, string Y, string Z)? ReadFamilyOriginPlanes(Document definition) {
        return ReadFamilyOriginFacts(definition).Full;
    }

    // Для проверки замка сохраняет подписанное смещение плоскости ширины в локальных координатах.
    private static ((string X, string Y, string Z)? Full, (string Name, double Offset)? Width) ReadFamilyOriginFacts(
        Document definition) {
        using var collector = new FilteredElementCollector(definition);
        var origins = new Dictionary<int, (string Name, double Offset)>();
        var axes = new[] { XYZ.BasisX, XYZ.BasisY, XYZ.BasisZ };
        bool complete = true;
        bool widthValid = true;
        var planes = collector.OfClass(typeof(ReferencePlane)).Cast<ReferencePlane>().ToList();
        foreach(var datum in planes) {
            if(datum.GetParamValue<int>(BuiltInParameter.DATUM_PLANE_DEFINES_ORIGIN) != 1) {
                continue;
            }
            using var plane = datum.GetPlane();
            int axis = Array.FindIndex(axes, v => plane.Normal.CrossProduct(v).GetLength() <= DirectionEpsilon);
            if(axis < 0 || origins.ContainsKey(axis) || string.IsNullOrEmpty(datum.Name)
                || !IsFinitePoint(plane.Origin)) {
                complete = false;
                widthValid &= axis != 0;
                continue;
            }
            double offset = plane.Origin.DotProduct(axes[axis]);
            origins.Add(axis, (datum.Name, offset));
            complete &= Math.Abs(offset) <= LengthEpsilon;
        }
        (string Name, double Offset)? width = widthValid && origins.TryGetValue(0, out var x)
            && planes.Count(plane => plane.Name == x.Name) == 1 ? x : null;
        return (complete && origins.Count == 3 ? (origins[0].Name, origins[1].Name, origins[2].Name) : null, width);
    }

    // Кеширует числовые оси и повторяемый отказ для одного хоста в пределах текущего запроса.
    public WallFrame GetWallFrame(Wall wall) {
        if(wall is null) {
            return null;
        }
        if(!_wallFrames.TryGetValue(wall.Id, out var frame)) {
            frame = ReadOptional(() => CreateWallFrame(wall));
            _wallFrames.Add(wall.Id, frame);
        }
        return frame;
    }

    // Начало X задаёт направление LocationCurve, положение по Y определяют внешние грани.
    private static WallFrame CreateWallFrame(Wall wall) {
        if(wall.WallType.Kind != WallKind.Basic || wall.CrossSection != WallCrossSection.Vertical
            || wall.IsStackedWallMember || wall.Location is not LocationCurve location
            || location.Curve is not Line line || Math.Abs(line.Direction.Z) > DirectionEpsilon) {
            return null;
        }
        var origin = line.GetEndPoint(0);
        var x = new XYZ(line.Direction.X, line.Direction.Y, 0).Normalize();
        var y = XYZ.BasisZ.CrossProduct(x);
        double? exterior = GetSideOffset(wall, ShellLayerType.Exterior, origin, y);
        double? interior = GetSideOffset(wall, ShellLayerType.Interior, origin, y);
        if(!exterior.HasValue || !interior.HasValue) {
            return null;
        }
        double thickness = Math.Abs(exterior.Value - interior.Value);
        if(thickness <= LengthEpsilon || Math.Abs(thickness - wall.Width) > LengthEpsilon) {
            return null;
        }
        return new WallFrame(AsPoint(origin + y * ((exterior.Value + interior.Value) / 2)), x.X, x.Y, thickness);
    }

    // Несколько фрагментов одной стороны допустимы только в одной плоскости.
    private static double? GetSideOffset(Wall wall, ShellLayerType side, XYZ origin, XYZ normal) {
        double? offset = null;
        foreach(var reference in HostObjectUtils.GetSideFaces(wall, side)) {
            if(wall.GetGeometryObjectFromReference(reference) is not PlanarFace face
                || face.FaceNormal.CrossProduct(normal).GetLength() > DirectionEpsilon) {
                return null;
            }
            double distance = (face.Origin - origin).DotProduct(normal);
            if(offset.HasValue && Math.Abs(distance - offset.Value) > LengthEpsilon) {
                return null;
            }
            offset = distance;
        }
        return offset;
    }

    // Индексирует грани один раз для всех вставок стены, не использует геометрию дверного полотна.
    public ILookup<ElementId, Face> GetRevealFaces(Wall wall) {
        if(!_reveals.TryGetValue(wall.Id, out var faces)) {
            var solids = GetSolids(wall, new[] { wall });
            faces = solids?.SelectMany(solid => solid.Faces.Cast<Face>())
                .Select(face => (Face: face, Ids: wall.GetGeneratingElementIds(face)))
                .Where(item => item.Ids.Count == 1 && item.Ids.First() != wall.Id)
                .ToLookup(item => item.Ids.First(), item => item.Face);
            _reveals.Add(wall.Id, faces);
        }
        return faces;
    }
}
