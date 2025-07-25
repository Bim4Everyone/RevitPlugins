using System;
using System.Collections.Generic;
using System.Threading;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing;
internal interface ISleevePlacerService {
    ICollection<SleeveModel> PlaceSleeves(
        ICollection<SleevePlacingOpts> opts,
        IProgress<int> progress,
        CancellationToken ct);
}
