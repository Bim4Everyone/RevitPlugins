using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Utilites;

namespace RevitBuildCoordVolumes.Models.Services;
internal class SlabNormalizeService : ISlabNormalizeService {

    public List<Face> GetTopFaces(SlabElement slabElement) {
        var floor = slabElement.Floor;
        var transform = slabElement.Transform;
        var commonSolids = floor.GetSolids();

        var transformedSolids = (transform == null)
            ? commonSolids
            : commonSolids.Select(solid => SolidUtils.CreateTransformed(solid, transform));

        var splittedSolids = transformedSolids.SelectMany(SolidUtils.SplitVolumes);

        return splittedSolids
            .SelectMany(solid => solid.Faces
                .Cast<Face>()
                .Where(face => IsFaceNormalWithinZRange(face, 0, 1)))
            .ToList();
    }

    public List<Face> GetTopFacesClean(SlabElement slabElement, List<Face> topFaces) {
        var floorProfile = slabElement.Profile;
        var linkTransform = slabElement.Transform;
        try {
            var contour = GetOuterContours(floorProfile, topFaces, linkTransform);
            var solid = SolidUtility.ExtrudeSolid(contour, start: 0, finish: 1, up: false);

            if(solid == null) {
                return topFaces;
            }

            var cleanFaces = solid.Faces
                .Cast<Face>()
                .Where(resultFace => IsFaceNormalWithinZRange(resultFace, 0, 1))
                .ToList();

            return cleanFaces;

        } catch {
            return topFaces;
        }
    }

    private bool IsFaceNormalWithinZRange(Face face, double minValue, double maxValue) {
        double normalZ = face.ComputeNormal(new UV(0.5, 0.5)).Normalize().Z;
        return normalZ > minValue && normalZ <= maxValue;
    }

    private List<CurveLoop> GetOuterContours(CurveArrArray curveArrArray, List<Face> topFaces, Transform linkTransform) {
        var outerContours = ExtractOuterCurveArrays(curveArrArray);
        double maxZ = GetTopFacesMaxZ(topFaces);
        return BuildCurveLoops(outerContours, linkTransform, maxZ);
    }

    private List<CurveArray> ExtractOuterCurveArrays(CurveArrArray curveArrArray) {
        var outerContours = new List<CurveArray>();

        for(int i = 0; i < curveArrArray.Size; i++) {
            var current = curveArrArray.get_Item(i);
            var samplePoint = current.get_Item(0).GetEndPoint(0);

            if(!IsInsideAnyOtherLoop(samplePoint, curveArrArray, i)) {
                outerContours.Add(current);
            }
        }

        return outerContours;
    }

    private bool IsInsideAnyOtherLoop(XYZ point, CurveArrArray allLoops, int currentIndex) {
        for(int j = 0; j < allLoops.Size; j++) {
            if(j == currentIndex) {
                continue;
            }

            if(IsPointInsideCurveArray(point, allLoops.get_Item(j))) {
                return true;
            }
        }
        return false;
    }

    private double GetTopFacesMaxZ(List<Face> topFaces) {
        double maxZ = double.MinValue;

        foreach(var face in topFaces) {
            foreach(EdgeArray edgeLoop in face.EdgeLoops) {
                double edgeZ = GetEdgeLoopZ(edgeLoop);
                if(edgeZ > maxZ) {
                    maxZ = edgeZ;
                }
            }
        }

        return maxZ;
    }

    private double GetEdgeLoopZ(EdgeArray edgeArray) {
        var enumerator = edgeArray.GetEnumerator();
        try {
            if(enumerator.MoveNext()) {
                var edge = (Edge) enumerator.Current;
                return edge.AsCurve().GetEndPoint(0).Z;
            }
        } finally {
            (enumerator as IDisposable)?.Dispose();
        }

        return double.MinValue;
    }

    private List<CurveLoop> BuildCurveLoops(
    List<CurveArray> outerContours,
    Transform linkTransform,
    double targetZ) {
        var result = new List<CurveLoop>();

        foreach(var curveArray in outerContours) {
            var projectedCurves = TransformAndProjectCurves(curveArray, linkTransform, targetZ);
            var orderedCurves = GeometryUtility.SortCurvesToLoop(projectedCurves);

            var loop = new CurveLoop();
            foreach(var curve in orderedCurves) {
                loop.Append(curve);
            }

            result.Add(loop);
        }

        return result;
    }

    private List<Curve> TransformAndProjectCurves(
    CurveArray curveArray,
    Transform transform,
    double z) {
        var curves = new List<Curve>();

        foreach(Curve original in curveArray) {
            var transformed = original.CreateTransformed(transform);

            var start = transformed.GetEndPoint(0);
            var end = transformed.GetEndPoint(1);

            var flattened = Line.CreateBound(
                new XYZ(start.X, start.Y, z),
                new XYZ(end.X, end.Y, z));

            curves.Add(flattened);
        }

        return curves;
    }

    private bool IsPointInsideCurveArray(XYZ point3d, CurveArray curveArray) {
        List<XYZ> poly = [];
        foreach(Curve item in curveArray) {
            var curve = item;
            poly.Add(new XYZ(curve.GetEndPoint(0).X, curve.GetEndPoint(0).Y, 0.0));
        }
        var p = new XYZ(point3d.X, point3d.Y, 0.0);
        return GeometryUtility.IsPointInsidePolygon(p, poly);
    }
}
