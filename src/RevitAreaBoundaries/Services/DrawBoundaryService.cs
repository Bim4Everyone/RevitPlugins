using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitAreaBoundaries.Models.Enums;

namespace RevitAreaBoundaries.Services;

internal class DrawBoundaryService {
    
    public void DrawBoundaryOnView(View view, List<Curve> curves, ProgressService progressService) {
        var document = view.Document;
        var sketchPlane = SketchPlane.Create(document, view.GenLevel.Id);
        progressService?.BeginStage(ProgressType.DrawProcessing);
        int total = curves.Count;
        int processed = 0;
        int reported = 0;
        foreach(var curve in curves) {
            progressService?.CancellationToken.ThrowIfCancellationRequested();
            document.Create.NewAreaBoundaryLine(sketchPlane, curve, view as ViewPlan);
            processed++;
            int current = processed * 99 / total;
            if(current > 99) {
                current = 99;
            }

            if(current <= reported) {
                continue;
            }

            reported = current;
            progressService?.ProgressCount?.Report(reported);
        }
    }
    
}
