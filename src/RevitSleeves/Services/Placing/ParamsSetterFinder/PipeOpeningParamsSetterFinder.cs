using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using Ninject;
using Ninject.Syntax;

using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Placing.ParamsSetter;

namespace RevitSleeves.Services.Placing.ParamsSetterFinder;
internal class PipeOpeningParamsSetterFinder : IParamsSetterFinder<ClashModel<Pipe, FamilyInstance>> {
    private readonly IResolutionRoot _resolutionRoot;

    public PipeOpeningParamsSetterFinder(IResolutionRoot resolutionRoot) {
        _resolutionRoot = resolutionRoot ?? throw new System.ArgumentNullException(nameof(resolutionRoot));
    }


    public IParamsSetter GetParamsSetter(ClashModel<Pipe, FamilyInstance> param) {
        var clashParam = new Ninject.Parameters.ConstructorArgument("clash", param);
        return _resolutionRoot.Get<PipeOpeningParamsSetter>(clashParam);
    }
}
