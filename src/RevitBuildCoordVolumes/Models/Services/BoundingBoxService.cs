using System;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Interfaces;

namespace RevitBuildCoordVolumes.Models.Services;

public class BoundingBoxService : IBoundingBoxService{
    // Метод получения трансформированного BoundingBoxXYZ
    public BoundingBoxXYZ GetTransformedBoundingBox(BoundingBoxXYZ bbox, Transform transform) {
        var min = bbox.Min;
        var max = bbox.Max;

        double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;

        for(int x = 0; x <= 1; x++) {
            for(int y = 0; y <= 1; y++) {
                for(int z = 0; z <= 1; z++) {
                    var p = new XYZ(
                        x == 0 ? min.X : max.X,
                        y == 0 ? min.Y : max.Y,
                        z == 0 ? min.Z : max.Z);

                    var tp = transform.OfPoint(p);

                    minX = Math.Min(minX, tp.X);
                    minY = Math.Min(minY, tp.Y);
                    minZ = Math.Min(minZ, tp.Z);

                    maxX = Math.Max(maxX, tp.X);
                    maxY = Math.Max(maxY, tp.Y);
                    maxZ = Math.Max(maxZ, tp.Z);
                }
            }
        }

        return new BoundingBoxXYZ {
            Min = new XYZ(minX, minY, minZ),
            Max = new XYZ(maxX, maxY, maxZ)
        };
    }
}
