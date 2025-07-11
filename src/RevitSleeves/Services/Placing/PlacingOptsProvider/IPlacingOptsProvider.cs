using System.Collections.Generic;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.PlacingOptsProvider;
internal interface IPlacingOptsProvider<T> where T : class {
    ICollection<SleevePlacingOpts> GetOpts(ICollection<T> @params);
}
