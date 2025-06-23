using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.ParamsSetter;
internal class PipeFloorParamsSetter : IParamsSetter<ClashModel<Pipe, Floor>> {
    public void SetParamValues(FamilyInstance sleeve) {
        throw new NotImplementedException();
    }
}
