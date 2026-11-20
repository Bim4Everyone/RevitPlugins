using System.Collections.Generic;

using Autodesk.Revit.DB.Architecture;

using dosymep.Revit;

using RevitEnumerateBySpline.Models.Enums;
using RevitEnumerateBySpline.Models.Interfaces;
using RevitEnumerateBySpline.Models.Warnings;

namespace RevitEnumerateBySpline.Models.Services;

internal sealed class SpatialElementCheckService : ISpatialElementCheckService {
    public IReadOnlyList<WarningElement> CheckSpatialElements(IEnumerable<SpatialModel?> spatialModels) {
        var warnings = new List<WarningElement>();

        foreach(var spatialModel in spatialModels) {
            if(spatialModel?.SpatialElement is not Room room) {
                continue;
            }

            if(room.IsRedundant()) {
                warnings.Add(new WarningRedundantElement {
                    WarningType = WarningType.Redundant,
                    SpatialModel = spatialModel
                });
            } else if(room.IsNotEnclosed()) {
                warnings.Add(new WarningNotEnclosedElement {
                    WarningType = WarningType.NotEnclosed,
                    SpatialModel = spatialModel
                });
            }
        }

        return warnings;
    }
}
