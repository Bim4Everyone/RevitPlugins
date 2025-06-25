using Ninject;
using Ninject.Syntax;

using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Placing.ParamsSetter;

namespace RevitSleeves.Services.Placing.ParamsSetterFinder;
internal class MergeModelParamsSetterFinder : IParamsSetterFinder<SleeveMergeModel> {
    private readonly IResolutionRoot _resolutionRoot;

    public MergeModelParamsSetterFinder(IResolutionRoot resolutionRoot) {
        _resolutionRoot = resolutionRoot ?? throw new System.ArgumentNullException(nameof(resolutionRoot));
    }


    public IParamsSetter GetParamsSetter(SleeveMergeModel param) {
        var sleeveModelParam = new Ninject.Parameters.ConstructorArgument("sleeveModel", param);
        return _resolutionRoot.Get<MergeModelParamsSetter>(sleeveModelParam);
    }
}
