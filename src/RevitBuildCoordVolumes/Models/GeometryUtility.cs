using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models;
public static class GeometryUtility {


    public static bool IsPointInsidePolygon(XYZ p, List<XYZ> polygon) {
        bool inside = false;
        for(int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++) {
            if(((polygon[i].Y > p.Y) != (polygon[j].Y > p.Y)) &&
                (p.X < (polygon[j].X - polygon[i].X) * (p.Y - polygon[i].Y) /
                 (polygon[j].Y - polygon[i].Y) + polygon[i].X)) {
                inside = !inside;
            }
        }
        return inside;
    }

}
