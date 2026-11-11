using System.Collections.Generic;

using RevitEnumerateBySpline.Models.Warnings;

namespace RevitEnumerateBySpline.Models.Interfaces;

internal interface ISpatialElementCheckService {
    /// <summary>Находит избыточные и неограждённые помещения.</summary>
    IReadOnlyList<WarningElement> CheckSpatialElements(IEnumerable<SpatialModel?> spatialModels);
}
