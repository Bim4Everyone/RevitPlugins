using System;
using System.Collections.Generic;
using System.Threading;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing;
internal interface ISleeveCleanupService {
    /// <summary>
    /// Удаляет новые гильзы, которые дублируют старые гильзы
    /// </summary>
    /// <param name="oldSleeves">Старые гильзы</param>
    /// <param name="newSleeves">Новые гильзы</param>
    /// <param name="progress">Прогресс</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Новые гильзы, оставшиеся после удаления</returns>
    ICollection<SleeveModel> CleanupSleeves(
        ICollection<SleeveModel> oldSleeves,
        ICollection<SleeveModel> newSleeves,
        IProgress<int> progress,
        CancellationToken ct);
}
