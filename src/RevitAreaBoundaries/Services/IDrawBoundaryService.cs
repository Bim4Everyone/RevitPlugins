using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitAreaBoundaries.Services;

internal interface IDrawBoundaryService {
    void DrawBoundaryOnView(View view, List<Curve> curves, ProgressService progressService);
}
