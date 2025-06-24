using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Placing.ParamsSetter;

namespace RevitSleeves.Services.Placing.ParamsSetterFinder;
internal class PipeFloorParamsSetterFinder : IParamsSetterFinder<ClashModel<Pipe, Floor>> {
    public IParamsSetter GetParamsSetter(ClashModel<Pipe, Floor> param) {
        return new PipeFloorParamsSetter(param);
    }
}
