using RevitBuildCoordVolumes.Models.Interfaces;

namespace RevitBuildCoordVolumes.Models.Services;
internal class SpatialElementCheckService {
    private readonly IContourService _contourService;

    public SpatialElementCheckService(ContourService contourService) {
        _contourService = contourService;
    }

    //public IReadOnlyCollection<WarningElement> CheckSpatialObjects(List<SpatialObject> spatialObjects) {
    //    _contourService.GetSimpleCurveLoops
    //}
}
