using System.Collections.Generic;
using System.Linq;

using RevitBuildCoordVolumes.Models.Geometry;

namespace RevitBuildCoordVolumes.Models.Interfaces;
internal interface IColumnFactory {
    IEnumerable<IGrouping<string, Column>> GenerateColumnGroups(List<Polygon> polygons, List<SlabElement> slabs);
}
