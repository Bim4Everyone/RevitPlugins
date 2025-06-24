using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Placing.ParamsSetter;

namespace RevitSleeves.Services.Placing.ParamsSetterFinder;
internal class PipeWallParamsSetterFinder : IParamsSetterFinder<ClashModel<Pipe, Wall>> {
    public IParamsSetter GetParamsSetter(ClashModel<Pipe, Wall> param) {
        return new PipeWallParamsSetter(param);
    }
}
