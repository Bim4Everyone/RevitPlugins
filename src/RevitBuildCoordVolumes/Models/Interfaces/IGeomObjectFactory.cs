using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Geometry;

namespace RevitBuildCoordVolumes.Models.Interfaces;
internal interface IGeomObjectFactory {
    GeomObject GetSimpleGeomObject(SpatialElement spatialElement, double startExtrudePosition, double finishExtrudePosition);
    GeomObject GetUnitedGeomObject(List<ColumnObject> columns, double spatialElementPosition);
    GeomObject GetSeparatedGeomObject(List<ColumnObject> columns, double spatialElementPosition);
}
