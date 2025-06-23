using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.ParamsSetter;
internal class PipeOpeningParamsSetter : IParamsSetter<ClashModel<Pipe, FamilyInstance>> {
    public void SetParamValues(FamilyInstance sleeve) {
        throw new NotImplementedException();
    }
}
