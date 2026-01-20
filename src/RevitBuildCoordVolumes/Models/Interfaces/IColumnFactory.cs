using System.Collections.Generic;
using System.Linq;

using RevitBuildCoordVolumes.Models.Geometry;

namespace RevitBuildCoordVolumes.Models.Interfaces;
internal interface IColumnFactory {
    IEnumerable<IGrouping<string, ColumnObject>> GenerateColumnGroups(List<PolygonObject> polygons, List<SlabElement> slabs);
}
