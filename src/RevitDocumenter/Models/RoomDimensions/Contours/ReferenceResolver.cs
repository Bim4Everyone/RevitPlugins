using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitDocumenter.Models.RoomDimensions.Geometry;

namespace RevitDocumenter.Models.RoomDimensions.Contours;

/// <summary>
/// Находит привязки (Reference) для элементов контура по геометрии элементов модели.
/// </summary>
internal class ReferenceResolver {
    // Допуск попадания линии контура в плоскость грани и точки на ребро: 1 мм в футах
    private const double _distanceTolerance = 1.0 / 304.8;
    // Допуск параллельности нормалей
    private const double _normalTolerance = 1e-4;

    private readonly RevitRepository _revitRepository;

    public ReferenceResolver(RevitRepository revitRepository) {
        _revitRepository = revitRepository.ThrowIfNull();
    }

    public void Resolve(RoomContour contour) {
        contour.ThrowIfNull();
        var geometryCache = new Dictionary<ElementId, List<GeometryItem>>();

        foreach(var loop in contour.Loops) {
            for(int position = 0; position < loop.Elements.Count; position++) {
                if(loop.Elements[position] is ContourLine line) {
                    ResolveLine(line, loop, position, geometryCache);
                }
            }
            foreach(var point in loop.Points) {
                ResolvePoint(point, geometryCache);
            }
        }
    }

    private void ResolveLine(
        ContourLine line,
        ContourLoop loop,
        int position,
        Dictionary<ElementId, List<GeometryItem>> cache) {
        if(line.IsArc) {
            line.ReferenceError = "Дуги не поддерживаются";
            return;
        }
        if(line.LinkSourceId != null && line.LinkSourceId != ElementId.InvalidElementId) {
            line.ReferenceError = "Элементы связей не поддерживаются";
            return;
        }

        var ownSource = GetSourceElement(line);

        // Разделитель помещений или другая линия модели
        if(ownSource is CurveElement curveElement) {
            line.Reference = curveElement.GeometryCurve?.Reference;
            line.ReferenceMethod = "CurveElement";
            if(line.Reference is null) {
                line.ReferenceError = "У линии нет Reference";
            }
            return;
        }

        // Revit не указывает источник у некоторых линий (например, торец отдельно стоящей стены).
        // Тогда ищем грань у элементов соседних линий контура.
        bool fromNeighbors = ownSource is null;
        var candidates = fromNeighbors
            ? GetNeighborSources(loop, position)
            : new List<Element> { ownSource };
        if(candidates.Count == 0) {
            line.ReferenceError = "Нет элемента-источника ни у линии, ни у соседних линий";
            return;
        }

        var (face, faceOwner) = FindCoplanarFace(line, candidates, cache);
        if(face != null) {
            line.Reference = face.Face.Reference;
            line.ReferenceMethod = (face.IsSymbolGeometry ? "SymbolFace" : "ElementFace")
                                   + (fromNeighbors ? "FromNeighbor" : string.Empty);
            if(fromNeighbors) {
                line.InferredSourceId = faceOwner.Id;
            }
            return;
        }

        // Запасной вариант: вертикальное ребро в конце линии (работает как точечная привязка)
        var (edge, edgeOwner) = FindVerticalEdge(candidates, line.Segment.End, cache);
        if(edge is null) {
            (edge, edgeOwner) = FindVerticalEdge(candidates, line.Segment.Start, cache);
        }
        if(edge != null) {
            line.Reference = edge.Edge.Reference;
            line.ReferenceMethod = "VerticalEdgeAtLineEnd" + (fromNeighbors ? "FromNeighbor" : string.Empty);
            if(fromNeighbors) {
                line.InferredSourceId = edgeOwner.Id;
            }
            return;
        }

        line.ReferenceError = $"Не найдены грань в плоскости линии и ребро на ее конце "
                              + $"({string.Join(", ", candidates.Select(c => c.GetType().Name))})";
    }

    private void ResolvePoint(ContourPoint point, Dictionary<ElementId, List<GeometryItem>> cache) {
        var sources = new[] { point.Previous, point.Next }
            .Select(GetSourceElement)
            .Where(e => e != null)
            .GroupBy(e => e.Id)
            .Select(g => g.First())
            .ToList();

        var (edge, _) = FindVerticalEdge(sources, point.Location, cache);
        if(edge is null) {
            point.ReferenceError = "Не найдено вертикальное ребро в точке";
            return;
        }
        point.Reference = edge.Edge.Reference;
        point.ReferenceMethod = edge.IsSymbolGeometry ? "SymbolVerticalEdge" : "ElementVerticalEdge";
    }

    private List<Element> GetNeighborSources(ContourLoop loop, int position) {
        return new[] { loop.GetPrevious(position), loop.GetNext(position) }
            .OfType<ContourLine>()
            .Select(GetSourceElement)
            .Where(e => e != null)
            .GroupBy(e => e.Id)
            .Select(g => g.First())
            .ToList();
    }

    /// <summary>
    /// Ищет вертикальную плоскую грань, в плоскости которой лежит линия контура.
    /// Предпочитает грань, смотрящую в помещение, затем с наибольшим перекрытием с линией.
    /// </summary>
    private static (GeometryItem Item, Element Owner) FindCoplanarFace(
        ContourLine line,
        IEnumerable<Element> elements,
        Dictionary<ElementId, List<GeometryItem>> cache) {
        GeometryItem best = null;
        Element bestOwner = null;
        double bestScore = double.MinValue;

        foreach(var element in elements) {
            foreach(var item in GetGeometry(element, cache)) {
                if(item.Face is null || item.Face.Reference is null) {
                    continue;
                }
                var normal3 = item.Transform.OfVector(item.Face.FaceNormal);
                if(Math.Abs(normal3.Z) > _normalTolerance) {
                    continue;
                }
                var normal = new Point2D(normal3.X, normal3.Y).Normalize();
                if(Math.Abs(Math.Abs(normal.Dot(line.InwardNormal)) - 1) > _normalTolerance) {
                    continue;
                }
                var origin3 = item.Transform.OfPoint(item.Face.Origin);
                var origin = new Point2D(origin3.X, origin3.Y);
                if(Math.Abs((line.Segment.Midpoint - origin).Dot(normal)) > _distanceTolerance) {
                    continue;
                }

                double overlap = GetOverlap(line.Segment, item);
                bool facesRoom = normal.Dot(line.InwardNormal) > 0;
                double score = (facesRoom ? 1e6 : 0) + overlap;
                if(score > bestScore) {
                    bestScore = score;
                    best = item;
                    bestOwner = element;
                }
            }
        }
        return (best, bestOwner);
    }

    /// <summary>
    /// Ищет вертикальное ребро, проходящее через точку плана.
    /// </summary>
    private static (GeometryItem Item, Element Owner) FindVerticalEdge(
        IEnumerable<Element> elements,
        Point2D location,
        Dictionary<ElementId, List<GeometryItem>> cache) {
        foreach(var element in elements) {
            foreach(var item in GetGeometry(element, cache)) {
                if(item.Edge?.Reference is null || item.Edge.AsCurve() is not Line edgeLine) {
                    continue;
                }
                var start = item.Transform.OfPoint(edgeLine.GetEndPoint(0));
                var end = item.Transform.OfPoint(edgeLine.GetEndPoint(1));
                var direction = (end - start).Normalize();
                if(Math.Abs(Math.Abs(direction.Z) - 1) > _normalTolerance) {
                    continue;
                }
                if(new Point2D(start.X, start.Y).DistanceTo(location) > _distanceTolerance) {
                    continue;
                }
                return (item, element);
            }
        }
        return (null, null);
    }

    private Element GetSourceElement(ContourLine line) {
        if(line?.SourceId is null || line.SourceId == ElementId.InvalidElementId) {
            return null;
        }
        return _revitRepository.Document.GetElement(line.SourceId);
    }

    /// <summary>
    /// Перекрытие линии контура и проекции грани на ее направление (футы).
    /// </summary>
    private static double GetOverlap(Segment2D segment, GeometryItem item) {
        var parameters = new List<double>();
        foreach(CurveLoop loop in item.Face.GetEdgesAsCurveLoops()) {
            foreach(var curve in loop) {
                var p = item.Transform.OfPoint(curve.GetEndPoint(0));
                parameters.Add(segment.ProjectParameter(new Point2D(p.X, p.Y)));
            }
        }
        if(parameters.Count == 0) {
            return 0;
        }
        double faceMin = parameters.Min();
        double faceMax = parameters.Max();
        return Math.Max(0, Math.Min(faceMax, segment.Length) - Math.Max(faceMin, 0));
    }

    /// <summary>
    /// Плоские грани и ребра элемента. Для семейств берется геометрия символа:
    /// ее Reference принимаются размерами, в отличие от геометрии экземпляра.
    /// </summary>
    private static List<GeometryItem> GetGeometry(Element element, Dictionary<ElementId, List<GeometryItem>> cache) {
        if(cache.TryGetValue(element.Id, out var cached)) {
            return cached;
        }

        var items = new List<GeometryItem>();
        var options = new Options {
            ComputeReferences = true,
            DetailLevel = ViewDetailLevel.Fine,
            IncludeNonVisibleObjects = false
        };
        var geometry = element.get_Geometry(options);
        if(geometry != null) {
            foreach(var geometryObject in geometry) {
                switch(geometryObject) {
                    case Solid solid:
                        AddSolid(items, solid, Transform.Identity, false);
                        break;
                    case GeometryInstance instance:
                        foreach(var symbolObject in instance.GetSymbolGeometry()) {
                            if(symbolObject is Solid symbolSolid) {
                                AddSolid(items, symbolSolid, instance.Transform, true);
                            }
                        }
                        break;
                }
            }
        }
        cache[element.Id] = items;
        return items;
    }

    private static void AddSolid(List<GeometryItem> items, Solid solid, Transform transform, bool isSymbol) {
        if(solid.Faces.Size == 0) {
            return;
        }
        foreach(Face face in solid.Faces) {
            if(face is PlanarFace planarFace) {
                items.Add(new GeometryItem(planarFace, null, transform, isSymbol));
            }
        }
        foreach(Edge edge in solid.Edges) {
            items.Add(new GeometryItem(null, edge, transform, isSymbol));
        }
    }

    private class GeometryItem {
        public GeometryItem(PlanarFace face, Edge edge, Transform transform, bool isSymbolGeometry) {
            Face = face;
            Edge = edge;
            Transform = transform;
            IsSymbolGeometry = isSymbolGeometry;
        }

        public PlanarFace Face { get; }
        public Edge Edge { get; }
        public Transform Transform { get; }
        public bool IsSymbolGeometry { get; }
    }
}
