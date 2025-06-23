using System;
using System.Collections.Generic;
using System.Threading;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing;
internal interface ISleeveMergeService {
    void MergeSleeves(ICollection<SleeveModel> sleevesToMerge, IProgress<int> progress, CancellationToken ct);
}
