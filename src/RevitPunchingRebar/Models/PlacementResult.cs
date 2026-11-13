using System.Collections.Generic;

namespace RevitPunchingRebar.Models;

/// <summary>
/// Итог расстановки каркасов.
/// </summary>
internal class PlacementResult {
    /// <summary>
    /// Количество пилонов, переданных на расстановку.
    /// </summary>
    public int PylonsCount { get; set; }

    /// <summary>
    /// Количество пилонов, у которых каркасы расставлены.
    /// </summary>
    public int ProcessedPylonsCount { get; set; }

    /// <summary>
    /// Количество созданных каркасов.
    /// </summary>
    public int CreatedFramesCount { get; set; }

    /// <summary>
    /// Пропущенные пилоны: описание пилона и причина.
    /// </summary>
    public List<string> SkippedPylons { get; } = [];
}
