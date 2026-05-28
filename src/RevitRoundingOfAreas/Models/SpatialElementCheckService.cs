using System.Collections.Generic;

using dosymep.Revit;

using RevitRoundingOfAreas.Models.Enums;
using RevitRoundingOfAreas.Models.Warnings;

namespace RevitRoundingOfAreas.Models;
internal class SpatialElementCheckService {

    public IReadOnlyCollection<WarningElement> CheckSpatialElements(List<SpatialElement> spatialElements) {
        if(spatialElements.Count == 0) {
            return [];
        }
        List<WarningElement> warnings = [];
        foreach(var spatialElement in spatialElements) {
            if(spatialElement.Room.IsRedundant()) {
                warnings.Add(new WarningRedundantElement {
                    WarningType = WarningType.Redundant,
                    SpatialElement = spatialElement
                });
            } else if(spatialElement.Room.IsNotEnclosed()) {
                warnings.Add(new WarningNotEnclosedElement {
                    WarningType = WarningType.NotEnclosed,
                    SpatialElement = spatialElement
                });
            }
        }
        return warnings;
    }
}
