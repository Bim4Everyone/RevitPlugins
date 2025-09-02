using System;
using System.Collections.Generic;
using System.Threading;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Update;
internal interface ISleeveUpdaterService {
    void UpdateSleeves(
        ICollection<SleeveModel> sleeves,
        IProgress<int> progress,
        CancellationToken ct);
}
