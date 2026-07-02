using System;
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
        
        var basePoint = GetBasePointPosition(document);
        var localTransform = Transform.CreateTranslation(-basePoint);
        
        var elements = GetElements(document, view, categorySet);
        
        // Срез с отступом 1200 мм от уровня
        double finalElevation = elevation + (800 / 304.8);
        var origin = new XYZ(0, 0, finalElevation);
        var positivePlane = Plane.CreateByNormalAndOrigin(new XYZ(0, 0, 1), origin);

        // Дополнительный срез +500 мм
        var negativeOrigin = new XYZ(0, 0, finalElevation + (500 / 304.8));
        var negativePlane = Plane.CreateByNormalAndOrigin(new XYZ(0, 0, -1), negativeOrigin);

        var curves = new List<Curve>();
        foreach(var element in elements) {
            var unitedSolid = GetUnitedSolid(element);
            if(unitedSolid == null) {
                continue;
            }
            var transformedSolid = SolidUtils.CreateTransformed(unitedSolid, localTransform);
            var resCurves = GetCurvesFromSolid(transformedSolid, positivePlane, negativePlane);
            curves.AddRange(resCurves);
        }
        
        // var curves = new List<Curve>();
        // foreach(var element in elements) {
        //     var solids = element.GetSolids().ToArray();
        //     var validSolids = solids
        //         .Where(s => s != null && s.Faces.Size > 0 && s.Edges.Size > 0)
        //         .ToList();
        //
        //     foreach(var solid in validSolids) {
        //         var transformedSolid = SolidUtils.CreateTransformed(solid, localTransform);
        //         var resCurves = GetCurvesFromSolid(transformedSolid, positivePlane, negativePlane);
        //         curves.AddRange(resCurves);
        //     }
        // }
        
        return curves;
    }
    
    // Метод получения смещения базовой точки
    private XYZ GetBasePointPosition(Document document) {
        var basePoint = new FilteredElementCollector(document)
            .OfCategory(BuiltInCategory.OST_ProjectBasePoint)
            .WhereElementIsNotElementType()
            .Cast<BasePoint>()
            .FirstOrDefault();
        return basePoint?.Position;
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
            if(solid == null) {
                return curves;
            }
        
            var resultSolid = BooleanOperationsUtils.CutWithHalfSpace(solid, positivePlane);
            if(resultSolid == null) {
                return curves;
            }
        
            var finalSolid = BooleanOperationsUtils.CutWithHalfSpace(resultSolid, negativePlane);
            if(finalSolid == null) {
                return curves;
            }
        
            var downFaces = new List<Face>();
            foreach(Face face in finalSolid.Faces) {
                if(face is not PlanarFace planarFace) {
                    continue;
                }
                if(Math.Abs(planarFace.FaceNormal.Z + 1.0) < 1e-6) {
                    downFaces.Add(planarFace);
                }
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
