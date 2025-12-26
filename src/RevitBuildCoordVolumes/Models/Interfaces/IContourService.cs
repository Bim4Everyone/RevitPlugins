using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Geometry;

namespace RevitBuildCoordVolumes.Models.Interfaces;
internal interface IContourService {
    List<CurveLoop> GetColumnsCurveLoops(List<Column> columns, double spatialElementPosition, double startExtrudePosition);
    List<CurveLoop> GetColumnListCurveLoops(Column column, double spatialElementPosition, double startExtrudePosition);
    List<CurveLoop> GetCurveLoopsContour(List<Curve> allCurves, Transform transform);
}
