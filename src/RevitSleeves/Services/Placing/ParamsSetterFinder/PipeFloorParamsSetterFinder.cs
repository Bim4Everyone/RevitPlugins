using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using Ninject;
using Ninject.Syntax;

using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Placing.ParamsSetter;

namespace RevitSleeves.Services.Placing.ParamsSetterFinder;
internal class PipeFloorParamsSetterFinder : IParamsSetterFinder<ClashModel<Pipe, Floor>> {
    private readonly IResolutionRoot _resolutionRoot;

    public PipeFloorParamsSetterFinder(IResolutionRoot resolutionRoot) {
        _resolutionRoot = resolutionRoot ?? throw new System.ArgumentNullException(nameof(resolutionRoot));
    }


    public IParamsSetter GetParamsSetter(ClashModel<Pipe, Floor> param) {
        var clashParam = new Ninject.Parameters.ConstructorArgument("clash", param);
        return _resolutionRoot.Get<PipeFloorParamsSetter>(clashParam);
    }
}
