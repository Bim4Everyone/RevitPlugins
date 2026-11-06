using System.Collections.Generic;

using RevitPunchingRebar.Models;
using RevitPunchingRebar.Models.Interfaces;

namespace RevitPunchingRebar.Services;

/// <summary>
/// Сервис расстановки каркасов поперечного армирования.
/// </summary>
internal interface IPunchingRebarPlacementService {
    /// <summary>
    /// Расставляет каркасы у каждого пилона.
    /// </summary>
    /// <remarks>
    /// Пилон, у которого расставить каркасы не удалось, пропускается, остальные обрабатываются.
    /// </remarks>
    /// <param name="pylons">Пилоны, у которых нужно расставить каркасы.</param>
    /// <param name="frameParams">Параметры каркасов.</param>
    /// <returns>Итог расстановки.</returns>
    PlacementResult Run(IEnumerable<IPylon> pylons, IFrameParams frameParams);
}
