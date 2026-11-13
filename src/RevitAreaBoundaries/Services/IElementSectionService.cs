using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitAreaBoundaries.Settings;

namespace RevitAreaBoundaries.Services;

internal interface IElementSectionService {
    List<Curve> GetSectionCurves(View view, AreaBoundarySettings areaBoundarySettings, ProgressService progressService);
}
