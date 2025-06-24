using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Placing.ParamsSetter;

namespace RevitSleeves.Services.Placing.ParamsSetterFinder;
internal class PipeOpeningParamsSetterFinder : IParamsSetterFinder<ClashModel<Pipe, FamilyInstance>> {
    public IParamsSetter GetParamsSetter(ClashModel<Pipe, FamilyInstance> param) {
        return new PipeOpeningParamsSetter(param);
    }
}
