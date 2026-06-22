using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitAreaBoundaries.Models;

namespace RevitAreaBoundaries.Services;

public class ElementSectionService(SystemPluginConfig systemPluginConfig) {

    public List<Curve> GetSectionCurves(Document document, View view) {
        var categorySet = new HashSet<BuiltInCategory>(systemPluginConfig.AllCategories);
        var level = view.GenLevel;
        double elevation = level.Elevation;
        
        var elements = GetElements(document, view, categorySet);
        
        // Уровень +1200 мм
        double finalElevation = elevation + (1200 / 304.8);
        var origin = new XYZ(0, 0, finalElevation);
        var positivePlane = Plane.CreateByNormalAndOrigin(new XYZ(0, 0, 1), origin);

        // Доп. срез +500 мм
        var negativeOrigin = new XYZ(0, 0, finalElevation + (500 / 304.8));
        var negativePlane = Plane.CreateByNormalAndOrigin(new XYZ(0, 0, -1), negativeOrigin);

        var curves = new List<Curve>();
        foreach(var element in elements) {
            var solid = GetUnitedSolid(element);
            var resCurves = GetCurvesFromSolid(solid, positivePlane, negativePlane);
            curves.AddRange(resCurves);
        }
        return curves;
    }

    private IEnumerable<Element> GetElements(Document document, View view, HashSet<BuiltInCategory> categorySet) {
        return new FilteredElementCollector(document, view.Id)
            .WhereElementIsNotElementType()
            .Where(element => ElementMatchesCategory(element, categorySet));
    }
    
    private bool ElementMatchesCategory(Element element, HashSet<BuiltInCategory> categories) {
        var category = element.Category?.GetBuiltInCategory();
        return category != null && categories.Contains(category.Value);
    }
    
    private Solid GetUnitedSolid(Element element) {
        var solids = element.GetSolids().ToArray();
        if(!solids.Any())
            return null;

        var unitedSolids = SolidExtensions.CreateUnitedSolids(solids);

        var validSolids = unitedSolids
            .Where(s => s != null && s.Faces.Size > 0 && s.Edges.Size > 0)
            .ToList();

        return validSolids
            .OrderByDescending(GetSafeSolidVolume)
            .FirstOrDefault();
    }

    private double GetSafeSolidVolume(Solid solid) {
        if(solid == null) return 0;
        try {
            return solid.Volume;
        } catch {
            return 0;
        }
    }
    
    private static List<Curve> GetCurvesFromSolid(Solid solid, Plane positivePlane, Plane negativePlane) {
        var curves = new List<Curve>();
        try {
            if(solid == null) return curves;

            var resultSolid = BooleanOperationsUtils.CutWithHalfSpace(solid, positivePlane);
            if(resultSolid == null) return curves;

            var finalSolid = BooleanOperationsUtils.CutWithHalfSpace(resultSolid, negativePlane);
            if(finalSolid == null) return curves;

            var downFaces = new List<Face>();
            foreach(Face face in finalSolid.Faces) {
                if(face is not PlanarFace planarFace) continue;
                if(planarFace.FaceNormal.Z == -1) downFaces.Add(planarFace);
            }

            foreach(var face in downFaces) {
                foreach(EdgeArray loop in face.EdgeLoops) {
                    foreach(Edge edge in loop) {
                        curves.Add(edge.AsCurve());
                    }
                }
            }
        } catch {
            // ignored
        }
        return curves;
    }
    
}
