using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing;
internal interface ISleevePlacingOptsProvider<T> where T : class {
    SleevePlacingOpts GetOpts(T param);
}
