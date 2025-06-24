using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Placing.ParamsSetter;

namespace RevitSleeves.Services.Placing.ParamsSetterFinder;
internal class MergeModelParamsSetterFinder : IParamsSetterFinder<SleeveMergeModel> {
    public IParamsSetter GetParamsSetter(SleeveMergeModel param) {
        return new MergeModelParamsSetter(param);
    }
}
