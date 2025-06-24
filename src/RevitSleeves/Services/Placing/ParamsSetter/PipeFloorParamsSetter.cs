using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.ParamsSetter;
internal class PipeFloorParamsSetter : IParamsSetter<ClashModel<Pipe, Floor>> {
    private readonly ClashModel<Pipe, Floor> _clash;

    public PipeFloorParamsSetter(ClashModel<Pipe, Floor> clash) {
        _clash = clash ?? throw new ArgumentNullException(nameof(clash));
    }


    public void SetParamValues(FamilyInstance sleeve) {
        throw new NotImplementedException();
    }
}
