using System.Collections.Generic;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Services;

namespace RevitBuildCoordVolumes.Models.Interfaces;

internal interface ISplitSolidService {
    /// <summary>
    /// Разбиение солидов на непересекающиеся части.
    /// </summary>
    /// <remarks>
    /// В данном методе исходные солиды последовательно сравниваются с уже сформированными частями результата.
    /// При обнаружении пересечения существующая часть и текущий солид разделяются на общую область
    /// и остатки без пересечения. В результате формируется набор валидных солидов без взаимного перекрытия.
    /// </remarks>
    /// <param name="solids">Исходный список солидов.</param>
    /// <param name="progressService">Прогресс-сервис</param>
    /// <returns>
    /// Список солидов, разбитых на непересекающиеся части.
    /// </returns>
    IList<SolidObject> SplitSolidObjects(IList<SolidObject> solids, ProgressService progressService);
}
