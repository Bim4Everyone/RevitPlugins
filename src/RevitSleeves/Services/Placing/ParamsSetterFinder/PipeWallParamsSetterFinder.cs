using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.ParamsSetterFinder;
internal class PipeWallParamsSetterFinder : IParamsSetterFinder<ClashModel<Pipe, Wall>> {
    public IParamsSetter GetParamsSetter(ClashModel<Pipe, Wall> param) {
        throw new NotImplementedException();
    }
}
