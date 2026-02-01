using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Geometry;

namespace RevitBuildCoordVolumes.Models.Interfaces;
internal interface IGeomObjectFactory {
    List<GeomObject> GetSimpleGeomObjects(SpatialElement spatialElement, double startExtrudePosition, double finishExtrudePosition);
    List<GeomObject> GetUnitedGeomObjects(List<ColumnObject> columns, double spatialElementPosition);
    List<GeomObject> GetSeparatedGeomObjects(List<ColumnObject> columns, double spatialElementPosition);
}
