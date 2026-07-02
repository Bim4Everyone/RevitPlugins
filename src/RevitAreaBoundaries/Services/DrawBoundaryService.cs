using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitAreaBoundaries.Models;

namespace RevitAreaBoundaries.Services;

public class DrawBoundaryService {
    
    public void DrawBoundaryOnView(View view, List<Curve> curves) {
        var document = view.Document;
        var sketchPlane = SketchPlane.Create(document, view.GenLevel.Id);
        
        foreach(var curve in curves) {
            document.Create.NewAreaBoundaryLine(sketchPlane, curve, view as ViewPlan);
        }
    }
    
}
