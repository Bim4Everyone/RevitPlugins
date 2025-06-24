using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.ParamsSetter;
internal class PipeWallParamsSetter : IParamsSetter<ClashModel<Pipe, Wall>> {
    private readonly ClashModel<Pipe, Wall> _clash;

    public PipeWallParamsSetter(ClashModel<Pipe, Wall> clash) {
        _clash = clash ?? throw new ArgumentNullException(nameof(clash));
    }


    public void SetParamValues(FamilyInstance sleeve) {
        throw new NotImplementedException();
    }
}
