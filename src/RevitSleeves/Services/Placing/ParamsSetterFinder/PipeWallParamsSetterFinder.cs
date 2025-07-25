using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using Ninject;
using Ninject.Syntax;

using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Placing.ParamsSetter;

namespace RevitSleeves.Services.Placing.ParamsSetterFinder;
internal class PipeWallParamsSetterFinder : IParamsSetterFinder<ClashModel<Pipe, Wall>> {
    private readonly IResolutionRoot _resolutionRoot;

    public PipeWallParamsSetterFinder(IResolutionRoot resolutionRoot) {
        _resolutionRoot = resolutionRoot ?? throw new System.ArgumentNullException(nameof(resolutionRoot));
    }


    public IParamsSetter GetParamsSetter(ClashModel<Pipe, Wall> param) {
        var clashParam = new Ninject.Parameters.ConstructorArgument("clash", param);
        return _resolutionRoot.Get<PipeWallParamsSetter>(clashParam);
    }
}
