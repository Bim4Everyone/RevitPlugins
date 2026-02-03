using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitBuildCoordVolumes.Models.Enums;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.Models.Services;

internal class SpatialElementCheckService : ISpatialElementCheckService {
    private readonly IContourService _contourService;

    public SpatialElementCheckService(IContourService contourService) {
        _contourService = contourService;
    }

    public IReadOnlyCollection<WarningElement> CheckSpatialObjects(
        BuildCoordVolumeSettings settings, RevitRepository revitRepository) {
        var spatialObjects = settings.SpatialObjects;
        if(!spatialObjects.Any()) {
            return [];
        }
        List<WarningElement> warnings = [];
        foreach(var spatialObject in spatialObjects) {
            var spatialElement = spatialObject.SpatialElement as Area;

            if(spatialElement.IsRedundant()) {
                warnings.Add(new WarningRedundantElement {
                    WarningType = WarningType.Redundant,
                    SpatialObject = spatialObject
                });
            } else {
                if(spatialElement.IsNotEnclosed()) {
                    warnings.Add(new WarningNotEnclosedElement {
                        WarningType = WarningType.NotEnclosed,
                        SpatialObject = spatialObject
                    });
                } else {
                    var contourCurves = _contourService.GetOuterContour(spatialElement);
                    var loops = _contourService.GetCurveLoopsContour(contourCurves, null);
                    bool isBrokenContour = !loops.Any();
                    if(isBrokenContour) {
                        warnings.Add(new WarningBrokenContourElement {
                            WarningType = WarningType.BrokenContour,
                            SpatialObject = spatialObject
                        });
                    }
                }
            }

            if(settings.AlgorithmType == AlgorithmType.ParamBasedAlgorithm) {
                foreach(var paramMap in settings.ParamMaps) {
                    if(paramMap.Type == ParamType.TopZoneParam) {
                        var param = paramMap.SourceParam;
                        double paramValue = revitRepository.GetPositionInFeet(spatialElement, param.Name);
                        if(double.IsNaN(paramValue)) {
                            warnings.Add(new WarningNotFilledParamElement {
                                WarningType = WarningType.NotFilledParam,
                                RevitParam = param,
                                SpatialObject = spatialObject
                            });
                        }
                    }
                    if(paramMap.Type == ParamType.BottomZoneParam) {
                        var param = paramMap.SourceParam;
                        double paramValue = revitRepository.GetPositionInFeet(spatialElement, param.Name);
                        if(double.IsNaN(paramValue)) {
                            warnings.Add(new WarningNotFilledParamElement {
                                WarningType = WarningType.NotFilledParam,
                                RevitParam = param,
                                SpatialObject = spatialObject
                            });
                        }
                    }
                }
            }
        }
        return warnings;
    }
}
