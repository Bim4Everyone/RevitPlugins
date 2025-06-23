using System;
using System.Collections.Generic;
using System.Threading;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing;
internal interface ISleevePlacingOptsService {
    ICollection<SleevePlacingOpts> GetOpts(IProgress<int> progress, CancellationToken ct);
}
