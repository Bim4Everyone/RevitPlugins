using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitBuildCoordVolumes.Models;

internal class IntersectorFast {

    private readonly Document _document;
    private readonly List<FloorInfo> _floors = [];

    public IntersectorFast(Document document) {
        _document = document;
        CollectFloorsFromLinks();
    }

    /// <summary>
    /// Возвращает верхнюю Z ближайшего перекрытия над точкой XY
    /// </summary>
    public double GetIntersectLocation(XYZ origin) {
        double bestZ = origin.Z;

        foreach(var floor in _floors) {
            if(IsPointInsidePolygon(origin, floor.Contour)) {
                double topZ = floor.TopZ;
                if(topZ > origin.Z) {
                    bestZ = Math.Max(bestZ, topZ);
                }
            }
        }

        return bestZ;
    }

    private bool IsPointInsidePolygon(XYZ point, List<XYZ> polygon) {
        bool inside = false;
        for(int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++) {
            if(((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y)) &&
                (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) /
                           (polygon[j].Y - polygon[i].Y) + polygon[i].X)) {
                inside = !inside;
            }
        }
        return inside;
    }

    private void CollectFloorsFromLinks() {
        var linkInstances = new FilteredElementCollector(_document)
            .OfClass(typeof(RevitLinkInstance))
            .Cast<RevitLinkInstance>()
            .Where(l => l.GetLinkDocument() != null)
            .ToList();

        int totalFound = 0;

        foreach(var link in linkInstances) {
            var linkDoc = link.GetLinkDocument();
            var transform = link.GetTotalTransform();

            var collector = new FilteredElementCollector(linkDoc)
                .WherePasses(new ElementMulticategoryFilter(RevitConstants.SlabCategories))
                .WhereElementIsNotElementType()
                .ToElements();

            foreach(var elem in collector) {
                string typeName = elem.Name ?? "";
                if(!RevitConstants.SlabTypeNames.Any(s => typeName.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0)) {
                    continue;
                }

                var contour = GetExternalContour(elem, transform);
                if(contour == null || contour.Count < 3) {
                    continue;
                }

                double topZ = GetElementTopZ(elem, contour);

                _floors.Add(new FloorInfo { Contour = contour, TopZ = topZ });
                totalFound++;
            }
        }

        TaskDialog.Show("Intersector", $"Найдено плит и фундаментов: {totalFound}");
    }

    private List<XYZ> GetExternalContour(Element elem, Transform transform) {
        if(elem is not Floor floor) {
            return null;
        }

        // Берём Sketch через зависимые элементы
        var sketchIds = floor.GetDependentElements(new ElementClassFilter(typeof(Sketch)));
        if(sketchIds == null || !sketchIds.Any()) {
            return null;
        }

        if(_document.GetElement(sketchIds.First()) is not Sketch sketch) {
            return null;
        }

        var points = new List<XYZ>();

        // В Sketch ищем ModelCurve (кривые Sketch)
        foreach(var id in sketch.GetDependentElements(new ElementClassFilter(typeof(ModelCurve)))) {
            if(_document.GetElement(id) is not ModelCurve modelCurve) {
                continue;
            }

            var curve = modelCurve.GeometryCurve;
            if(curve == null) {
                continue;
            }

            // Берём только начальную точку кривой
            var p0 = transform.OfPoint(curve.GetEndPoint(0));
            if(!points.Any(pt => pt.IsAlmostEqualTo(p0))) {
                points.Add(p0);
            }
        }

        return points.Count >= 3 ? points : null;
    }

    private double GetElementTopZ(Element elem, List<XYZ> contour) {
        double topZ = double.NaN;
        if(elem.Document.GetElement(elem.LevelId) is Level lvl) {
            double offset = elem.LookupParameter("Смещение от уровня")?.AsDouble() ?? 0;
            topZ = lvl.Elevation + offset;
        }

        if(double.IsNaN(topZ)) {
            topZ = contour.Average(p => p.Z);
        }

        return topZ;
    }

    private class FloorInfo {
        public List<XYZ> Contour { get; set; }
        public double TopZ { get; set; }
    }

    private class XYZComparer : IEqualityComparer<XYZ> {
        private const double _tolerance = 1e-9;
        public bool Equals(XYZ a, XYZ b) {
            return a.DistanceTo(b) < _tolerance;
        }

        public int GetHashCode(XYZ obj) {
            return 0;
        }
    }
}
