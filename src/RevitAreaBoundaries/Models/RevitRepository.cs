using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.Revit.Geometry;

namespace RevitAreaBoundaries.Models;

internal class RevitRepository {
    
    private static readonly ICollection<BuiltInCategory> _allCategories = [
        BuiltInCategory.OST_Doors,
        BuiltInCategory.OST_Floors,
        BuiltInCategory.OST_Railings,
        BuiltInCategory.OST_RailingSystem,
        BuiltInCategory.OST_StairsRailing,
        BuiltInCategory.OST_StairsRailingBaluster,
        BuiltInCategory.OST_StairsRailingRail,
        BuiltInCategory.OST_RailingBalusterRail,
        BuiltInCategory.OST_RailingBalusterRailCut,
        BuiltInCategory.OST_RailingHandRail,
        BuiltInCategory.OST_RailingHandRailAboveCut,
        BuiltInCategory.OST_Roofs,
        BuiltInCategory.OST_Walls,
        BuiltInCategory.OST_Windows,
        BuiltInCategory.OST_GenericModel];
    
    public RevitRepository(UIApplication uiApplication) {
        UiApplication = uiApplication;
    }

    private UIApplication UiApplication { get; }
    private UIDocument ActiveUiDocument => UiApplication.ActiveUIDocument;
    public Application Application => UiApplication.Application;
    public Document Document => ActiveUiDocument.Document;
    
    // Метод фильтрации элементов по рабочему набору и категориям
    private bool ElementMatchesCategory(Element element, HashSet<BuiltInCategory> categories) {
        var category = element.Category?.GetBuiltInCategory();
        return category != null && categories.Contains(category.Value);
    }
    

    public void Action() {
        // Получаем активный вид
        var activeView = ActiveUiDocument.ActiveView;
        
        var categorySet = new HashSet<BuiltInCategory>(_allCategories);
        
        // Получаем все элементы на виде
        var elements = new FilteredElementCollector(Document, activeView.Id)
            .WhereElementIsNotElementType()
            .Where(element => ElementMatchesCategory(element, categorySet));
        
        // Получаем уровень
        var level = activeView.GenLevel;

        double elevation = level.Elevation;
        
        // Получаем смещение от уровня +1200. Строим Plane
        double finalElevation = elevation + (1200 / 304.8);
        var origin = new XYZ(0, 0, finalElevation);
        var positivePlane = Plane.CreateByNormalAndOrigin(new XYZ(0, 0, 1), origin);
        var negativeOrigin = new XYZ(0, 0, finalElevation + (500 / 304.8));
        var negativePlane = Plane.CreateByNormalAndOrigin(new XYZ(0, 0, -1), negativeOrigin);

        var curves = new List<Curve>();
        foreach(var element in elements) {
            var solid = GetUnitedSolid(element);
            var resCurves = GetCurvesFromSolid(solid, positivePlane, negativePlane, activeView);
            curves.AddRange(resCurves);
        }
        
        foreach(var curve in curves) {
            Document.Create.NewDetailCurve(activeView, curve);
        }

        
        
        System.Windows.MessageBox.Show(curves.Count.ToString());

    }
    
    public static bool IsPointInsidePolygon(XYZ point, List<XYZ> polygon) {
        bool inside = false;
        for(int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++) {
            if((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y) &&
               point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) /
               (polygon[j].Y - polygon[i].Y) + polygon[i].X) {
                inside = !inside;
            }
        }
        return inside;
    }
    

    private List<Curve> GetCurvesFromSolid(Solid solid, Plane positivePlane, Plane negativePlane, View activeView) {
        var curves = new List<Curve>();
        try {
            var resultSolid = BooleanOperationsUtils.CutWithHalfSpace(solid, positivePlane);
            if(resultSolid == null) {
                return [];
            }

            var finalSolid = BooleanOperationsUtils.CutWithHalfSpace(resultSolid, negativePlane);
            if(finalSolid == null) {
                return [];
            }

            var downFaces = new List<Face>();
            foreach(Face face in finalSolid.Faces) {
                if(face is not PlanarFace planarFace) {
                    continue;
                }

                if(planarFace.FaceNormal.Z == -1) {
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

    
    // Метод получения объединенного солида
    private Solid GetUnitedSolid(Element element) {
        var solids = element.GetSolids().ToArray();
        if(!solids.Any()) {
            return null;
        }
        var unitedSolids = SolidExtensions.CreateUnitedSolids(solids);
        
        var validSolids = unitedSolids
            .Where(s => s != null && s.Faces.Size > 0 && s.Edges.Size > 0)
            .ToList();

        return validSolids
            .OrderByDescending(GetSafeSolidVolume)
            .FirstOrDefault();
    }
    
    // Метод безопасного получения объёма солида
    private double GetSafeSolidVolume(Solid solid) {
        if(solid == null) {
            return 0;
        }
        try {
            return solid.Volume;
        } catch {
            return 0;
        }
    }
}
