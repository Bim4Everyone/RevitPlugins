using System;
using System.Collections.Generic;
using System.Threading;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing;
internal class SleevePlacerService : ISleevePlacerService {
    public SleevePlacerService() {

    }


    public ICollection<SleeveModel> PlaceSleeves(
        ICollection<SleevePlacingOpts> opts,
        IProgress<int> progress,
        CancellationToken ct) {

        return Array.Empty<SleeveModel>(); // TODO
    }
}
