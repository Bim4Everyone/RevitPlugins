using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Geometry;

namespace RevitBuildCoordVolumes.Models.Interfaces;
internal interface IContourService {
    List<CurveLoop> GetColumnsCurveLoops(List<ColumnObject> columns, double spatialElementPosition, double startExtrudePosition);
    List<CurveLoop> GetColumnCurveLoops(ColumnObject column, double spatialElementPosition, double startExtrudePosition);
    List<CurveLoop> GetSimpleCurveLoops(SpatialElement spatialElement, double startExtrudePosition);
}
