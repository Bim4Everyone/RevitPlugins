using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.ParamsSetterFinder;
internal class PipeFloorParamsSetterFinder : IParamsSetterFinder<ClashModel<Pipe, Floor>> {
    public IParamsSetter GetParamsSetter(ClashModel<Pipe, Floor> param) {
        throw new NotImplementedException();
    }
}
