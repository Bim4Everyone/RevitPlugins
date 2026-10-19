using System.Linq;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Utilites;

namespace RevitBuildCoordVolumes.Models.Services;

internal class SpatialSolidService (IContourService contourService, SystemPluginConfig config) : ISpatialSolidService{
    public Solid CreateSpatialSolid(SpatialElement spatialElement) {
        var contourCurves = contourService.GetOuterContour(spatialElement);
        var startContour = contourService.GetCurveLoopsContour(contourCurves, null);
        if(startContour.Count == 0) {
            return null;
        }
        double pointSpatial = startContour[0].ElementAt(0).GetEndPoint(0).Z;
        var transform = GeometryUtility.CreateZTranslation(pointSpatial, -config.ExtrusionLengthMm);
        foreach(var loop in startContour) {
            loop.Transform(transform);
        }
        return SolidUtility.ExtrudeSolid(startContour, -config.ExtrusionLengthMm, config.ExtrusionLengthMm);
    }
}
