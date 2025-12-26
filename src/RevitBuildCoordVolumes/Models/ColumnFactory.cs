using System.Collections.Generic;
using System.Linq;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;

namespace RevitBuildCoordVolumes.Models;
internal class ColumnFactory : IColumnFactory {

    public IEnumerable<IGrouping<string, Column>> GenerateColumnGroups(List<Polygon> polygons, List<SlabElement> slabs) {
        var columns = GetColumns(polygons, slabs);

        // Группируем по GUID плит - низ + верх      
        return columns
            .GroupBy(col => col.StartSlabGuid.ToString() + "_" + col.FinishSlabGuid.ToString());
    }

    private List<Column> GetColumns(List<Polygon> polygons, List<SlabElement> slabs) {
        if(polygons.Count == 0 || slabs.Count < 2) {
            return [];
        }
        var columns = new List<Column>();

        foreach(var polygon in polygons) {
            var center = polygon.Center;
            var points = new List<SlabInfo>(slabs.Count);

            foreach(var slab in slabs) {
                double z = double.NaN;

                foreach(var face in slab.UpFaces) {
                    var proj = face.Project(center);
                    if(proj != null) {
                        z = proj.XYZPoint.Z;
                        break;
                    }
                }

                if(!double.IsNaN(z)) {
                    points.Add(new SlabInfo {
                        Position = z,
                        SlabLevelName = slab.LevelName,
                        SlabGuid = slab.Guid,
                        IsSloped = slab.IsSloped
                    });
                }
            }

            if(points.Count < 2) {
                continue;
            }

            points.Sort((a, b) => a.Position.CompareTo(b.Position));

            var start = points[0];

            for(int i = 1; i < points.Count; i++) {
                var end = points[i];
                if(end.Position <= start.Position) {
                    continue;
                }

                columns.Add(new Column {
                    Polygon = polygon,
                    LevelName = start.SlabLevelName,
                    StartPosition = start.Position,
                    FinishPosition = end.Position,
                    StartSlabGuid = start.SlabGuid,
                    FinishSlabGuid = end.SlabGuid,
                    IsSloped = start.IsSloped || end.IsSloped
                });
                start = end;
            }
        }
        return columns;
    }
}
