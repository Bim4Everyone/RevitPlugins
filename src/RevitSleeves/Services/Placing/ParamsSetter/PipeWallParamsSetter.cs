using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.ParamsSetter;
internal class PipeWallParamsSetter : IParamsSetter<ClashModel<Pipe, Wall>> {
    public void SetParamValues(FamilyInstance sleeve) {
        throw new System.NotImplementedException();
    }
}
