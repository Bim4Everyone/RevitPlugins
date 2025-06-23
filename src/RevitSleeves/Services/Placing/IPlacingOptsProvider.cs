using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing;
internal interface IPlacingOptsProvider<T> where T : class {
    SleevePlacingOpts GetOpts(T param);
}
