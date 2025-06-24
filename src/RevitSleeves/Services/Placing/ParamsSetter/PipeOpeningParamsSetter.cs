using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.ParamsSetter;
internal class PipeOpeningParamsSetter : IParamsSetter<ClashModel<Pipe, FamilyInstance>> {
    private readonly ClashModel<Pipe, FamilyInstance> _clash;

    public PipeOpeningParamsSetter(ClashModel<Pipe, FamilyInstance> clash) {
        _clash = clash ?? throw new ArgumentNullException(nameof(clash));
    }


    public void SetParamValues(FamilyInstance sleeve) {
        throw new NotImplementedException();
    }
}
