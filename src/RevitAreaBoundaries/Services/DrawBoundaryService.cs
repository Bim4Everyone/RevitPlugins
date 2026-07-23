using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitAreaBoundaries.Models.Enums;

namespace RevitAreaBoundaries.Services;

internal class DrawBoundaryService {
    
    public void DrawBoundaryOnView(View view, List<Curve> curves, ProgressService progressService) {
        var document = view.Document;
        var sketchPlane = SketchPlane.Create(document, view.GenLevel.Id);
        progressService?.BeginStage(ProgressType.DrawProcessing);
        progressService?.BeginItemsProgress(curves.Count);
        foreach(var curve in curves) {
            progressService?.ThrowIfCancellationRequested();
            
            document.Create.NewAreaBoundaryLine(sketchPlane, curve, view as ViewPlan);
            
            progressService?.ReportNextItem();
        }
    }
}
