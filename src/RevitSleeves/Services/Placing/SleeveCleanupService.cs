using System;
using System.Collections.Generic;
using System.Threading;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing;
internal class SleeveCleanupService : ISleeveCleanupService {
    public SleeveCleanupService() {

    }


    public ICollection<SleeveModel> CleanupSleeves(
        ICollection<SleeveModel> oldSleeves,
        ICollection<SleeveModel> newSleeves,
        IProgress<int> progress,
        CancellationToken ct) {

        return Array.Empty<SleeveModel>(); // TODO
    }
}
