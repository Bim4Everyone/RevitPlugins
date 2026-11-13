using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitBuildCoordVolumes.Models.Interfaces;

namespace RevitBuildCoordVolumes.Models.Services;

public class SlabFaceService : ISlabFaceService {

    // Метод получения реальных верхних граней плиты
    public List<Face> GetRealTopFaces(Floor floor, Transform transformFromDoc) {
        var commonSolids = floor.GetSolids();

        var transformedSolids = transformFromDoc == null
            ? commonSolids
            : commonSolids.Select(solid =>
                SolidUtils.CreateTransformed(solid, transformFromDoc));

        return GetSolidsTopFaces(transformedSolids);
    }

    // Метод получения верхних граней плоской плиты
    public List<Face> GetFlatTopFaces(
        IEnumerable<(CurveLoop Loop, Solid Solid)> solids) {

        return solids
            .SelectMany(x => x.Solid.Faces.Cast<Face>())
            .Where(face => IsFaceNormalWithinZRange(face, 0, 1))
            .ToList();
    }

    // Метод получения самой верхней точки Face
    public double GetMaxPointZ(List<Face> topFaces) {
        double maxZ = double.MinValue;

        foreach (var face in topFaces) {
            foreach (EdgeArray edgeLoop in face.EdgeLoops) {
                foreach (Edge edge in edgeLoop) {
                    var curve = edge.AsCurve();

                    var p0 = curve.GetEndPoint(0);
                    var p1 = curve.GetEndPoint(1);

                    maxZ = Math.Max(maxZ, p0.Z);
                    maxZ = Math.Max(maxZ, p1.Z);
                }
            }
        }

        return maxZ;
    }

    // Метод получения самой нижней точки Face
    public double GetMinPointZ(List<Face> topFaces) {
        double minZ = double.MaxValue;

        foreach (var face in topFaces) {
            foreach (EdgeArray edgeLoop in face.EdgeLoops) {
                foreach (Edge edge in edgeLoop) {
                    var curve = edge.AsCurve();

                    var p0 = curve.GetEndPoint(0);
                    var p1 = curve.GetEndPoint(1);

                    minZ = Math.Min(minZ, p0.Z);
                    minZ = Math.Min(minZ, p1.Z);
                }
            }
        }

        return minZ;
    }

    // Метод получения верхних граней из любых солидов
    private static List<Face> GetSolidsTopFaces(IEnumerable<Solid> solids) {
        return solids
            .SelectMany(solid => solid.Faces
                .Cast<Face>()
                .Where(face => IsFaceNormalWithinZRange(face, 0, 1)))
            .ToList();
    }

    // Метод определения, входит ли значение нормали Face в заданный диапазон
    private static bool IsFaceNormalWithinZRange(
        Face face,
        double minValue,
        double maxValue) {

        double normalZ = face
            .ComputeNormal(new UV(0.5, 0.5))
            .Normalize()
            .Z;

        return normalZ > minValue && normalZ <= maxValue;
    }
}
