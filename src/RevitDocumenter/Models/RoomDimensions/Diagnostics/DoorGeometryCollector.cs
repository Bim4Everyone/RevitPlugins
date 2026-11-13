using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitDocumenter.Models.RoomDimensions.Geometry;

namespace RevitDocumenter.Models.RoomDimensions.Diagnostics;

/// <summary>
/// Выгружает доступные кривые геометрии двери для диагностики полотна и открывания.
/// Проекция ребер тела не гарантирует совпадения с видимой 2D-символикой: сверять с PNG.
/// </summary>
internal class DoorGeometryCollector {
    private const int _curveLimit = 256;
    private const int _pointLimit = 48;
    private const int _depthLimit = 8;
    private readonly Document _document;
    private readonly View _view;
    private readonly List<DoorSymbolCurve> _curves = [];
    private readonly List<string> _errors = [];
    private bool _truncated;

    public DoorGeometryCollector(Document document, View view) {
        _document = document.ThrowIfNull();
        _view = view.ThrowIfNull();
    }

    public DoorGeometryData Collect(Element door) {
        door.ThrowIfNull();
        _curves.Clear();
        _errors.Clear();
        _truncated = false;
        using var options = new Options { View = _view, IncludeNonVisibleObjects = false, ComputeReferences = false };
        var geometry = door.get_Geometry(options);
        if(geometry != null) {
            ReadGeometry(geometry, Transform.Identity, 0);
        }
        return new DoorGeometryData {
            Source = "API view geometry: curves, polylines and solid edges; not verified visible pixels",
            CurveLimit = _curveLimit,
            PointsPerCurveLimit = _pointLimit,
            Truncated = _truncated,
            Curves = _curves,
            Errors = _errors
        };
    }

    private void ReadGeometry(GeometryElement geometry, Transform transform, int depth) {
        if(depth > _depthLimit) {
            _truncated = true;
            return;
        }
        // Сначала самостоятельные кривые: символика важнее множества ребер трехмерного тела.
        var objects = geometry.Cast<GeometryObject>().ToList();
        foreach(var item in objects.OfType<Curve>()) {
            AddCurve(item, transform, "Curve");
        }
        foreach(var item in objects.OfType<PolyLine>()) {
            TryRead(() => AddPoints(item.GetCoordinates(), transform, "PolyLine", "PolyLine", item.GraphicsStyleId));
        }
        foreach(var instance in objects.OfType<GeometryInstance>()) {
            if(_curves.Count >= _curveLimit) {
                _truncated = true;
                return;
            }
            TryRead(() => ReadGeometry(
                instance.GetSymbolGeometry(), transform.Multiply(instance.Transform), depth + 1));
        }
        foreach(var solid in objects.OfType<Solid>()) {
            foreach(Edge edge in solid.Edges) {
                if(_curves.Count >= _curveLimit) {
                    _truncated = true;
                    return;
                }
                TryRead(() => AddCurve(edge.AsCurve(), transform, "SolidEdge"));
            }
        }
    }

    private void AddCurve(Curve curve, Transform transform, string source) {
        TryRead(() => AddPoints(curve.Tessellate(), transform, curve.GetType().Name, source, curve.GraphicsStyleId));
    }

    private void AddPoints(IList<XYZ> points, Transform transform, string kind, string source, ElementId styleId) {
        if(_curves.Count >= _curveLimit) {
            _truncated = true;
            return;
        }
        if(points.Count < 2) {
            return;
        }
        int count = Math.Min(points.Count, _pointLimit);
        var samples = Enumerable.Range(0, count)
            .Select(i => points[(int) Math.Round(i * (points.Count - 1.0) / (count - 1))])
            .Select(transform.OfPoint)
            .ToList();
        var style = _document.GetElement(styleId) as GraphicsStyle;
        _curves.Add(new DoorSymbolCurve {
            Kind = kind,
            Source = source,
            GraphicsCategory = style?.GraphicsStyleCategory?.Name,
            OriginalPointCount = points.Count,
            PointsTruncated = points.Count > count,
            ModelPointsMm = samples.Select(p => new[] { ToMm(p.X), ToMm(p.Y), ToMm(p.Z) }).ToList(),
            ViewPointsMm = samples.Select(p => new[] {
                ToMm((p - _view.Origin).DotProduct(_view.RightDirection)),
                ToMm((p - _view.Origin).DotProduct(_view.UpDirection))
            }).ToList()
        });
    }

    private void TryRead(Action read) {
        try {
            read();
        } catch(Exception ex) {
            if(_errors.Count < 20) {
                _errors.Add($"{ex.GetType().Name}: {ex.Message}");
            }
        }
    }

    private static double ToMm(double value) {
        return Math.Round(UnitUtilsHelper.ConvertFromInternalValue(value), 2);
    }
}
