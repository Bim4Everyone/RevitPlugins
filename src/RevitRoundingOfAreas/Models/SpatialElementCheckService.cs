using System.Collections.Generic;

using Autodesk.Revit.DB.Architecture;

using dosymep.Revit;

using RevitRoundingOfAreas.Models.Enums;
using RevitRoundingOfAreas.Models.Warnings;

namespace RevitRoundingOfAreas.Models;
internal class SpatialElementCheckService {

    public IReadOnlyCollection<WarningElement> CheckSpatialElements(List<SpatialModel> spatialModels) {
        if(spatialModels.Count == 0) {
            return [];
        }
        List<WarningElement> warnings = [];
        foreach(var spatialModel in spatialModels) {
            var room = spatialModel.SpatialElement as Room;
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
