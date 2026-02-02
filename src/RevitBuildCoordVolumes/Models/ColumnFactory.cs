using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;

namespace RevitBuildCoordVolumes.Models;
internal class ColumnFactory : IColumnFactory {
    // Длина линии для пресечения, примерно 100 этажей
    private const double _rayHeight = 1000;

    public IEnumerable<IGrouping<string, ColumnObject>> GenerateColumnGroups(List<PolygonObject> polygons, List<SlabElement> slabs) {
        var columns = GetColumns(polygons, slabs);

        // Группируем по GUID плит - низ + верх      
        return columns
            .GroupBy(col => col.StartSlabGuid.ToString() + "_" + col.FinishSlabGuid.ToString());
    }

    private List<ColumnObject> GetColumns(List<PolygonObject> polygons, List<SlabElement> slabs) {
        if(polygons.Count == 0 || slabs.Count < 2) {
            return [];
        }
        var columns = new List<ColumnObject>();

        foreach(var polygon in polygons) {
            var center = polygon.Center;
            var points = new List<SlabObject>(slabs.Count);
            var line = Line.CreateBound(center + XYZ.BasisZ * _rayHeight, center - XYZ.BasisZ * _rayHeight);

            foreach(var slab in slabs) {
                double z = double.NaN;

                foreach(var face in slab.TopFaces) {
                    var result = face.Intersect(line, out var ira);

                    if(result == SetComparisonResult.Overlap && ira != null && ira.Size > 0) {
                        var hit = ira.get_Item(0).XYZPoint;
                        z = hit.Z;
                    }
                }

                if(!double.IsNaN(z)) {
                    points.Add(new SlabObject {
                        Position = z,
                        FloorName = slab.LevelName,
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

                columns.Add(new ColumnObject {
                    PolygonObject = polygon,
                    FloorName = start.FloorName,
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
