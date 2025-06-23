using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.ParamsSetterFinder;
internal class PipeOpeningParamsSetterFinder : IParamsSetterFinder<ClashModel<Pipe, FamilyInstance>> {
    public IParamsSetter GetParamsSetter(ClashModel<Pipe, FamilyInstance> param) {
        throw new NotImplementedException();
    }
}
