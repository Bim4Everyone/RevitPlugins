using System;
using System.Collections.Generic;
using System.Threading;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing;
internal class SleeveMergeService : ISleeveMergeService {
    public void MergeSleeves(ICollection<SleeveModel> sleevesToMerge, IProgress<int> progress, CancellationToken ct) {
        throw new NotImplementedException();
    }
}
