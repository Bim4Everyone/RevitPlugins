using System.Collections.Generic;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing;
internal interface ISleevePlacingOptsService {
    ICollection<SleevePlacingOpts> GetOpts();
}
