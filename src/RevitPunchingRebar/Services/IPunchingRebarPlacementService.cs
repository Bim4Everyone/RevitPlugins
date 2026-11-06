using System.Collections.Generic;

using RevitPunchingRebar.Models.Interfaces;

namespace RevitPunchingRebar.Services;

/// <summary>
/// Сервис расстановки каркасов поперечного армирования.
/// </summary>
internal interface IPunchingRebarPlacementService {
    /// <summary>
    /// Расставляет каркасы у каждого пилона.
    /// </summary>
    /// <param name="pylons">Пилоны, у которых нужно расставить каркасы.</param>
    /// <param name="frameParams">Параметры каркасов.</param>
    void Run(IEnumerable<IPylon> pylons, IFrameParams frameParams);
}
