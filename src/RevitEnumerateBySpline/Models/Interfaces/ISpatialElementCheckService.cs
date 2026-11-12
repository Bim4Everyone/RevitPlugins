using System.Collections.Generic;

using RevitEnumerateBySpline.Models.Warnings;

namespace RevitEnumerateBySpline.Models.Interfaces;

/// <summary>Проверяет пригодность помещений к нумерации.</summary>
internal interface ISpatialElementCheckService {
    /// <summary>Находит избыточные и не окруженные помещения.</summary>
    /// <param name="spatialModels">Проверяемые помещения.</param>
    IReadOnlyList<WarningElement> CheckSpatialElements(IEnumerable<SpatialModel?> spatialModels);
}
