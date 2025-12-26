using System.Collections.Generic;

using RevitBuildCoordVolumes.Models.Geometry;

namespace RevitBuildCoordVolumes.Models.Interfaces;
internal interface IGeomElementFactory {
    GeomElement GetUnitedGeomElement(List<Column> columns, double spatialElementPosition);
    GeomElement GetSeparatedGeomElement(List<Column> columns, double spatialElementPosition);
}
