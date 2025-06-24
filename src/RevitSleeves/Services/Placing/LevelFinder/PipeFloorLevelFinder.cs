using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.LevelFinder;
internal class PipeFloorLevelFinder : ILevelFinder<ClashModel<Pipe, Floor>> {
    public Level GetLevel(ClashModel<Pipe, Floor> param) {
        return param.MepElement.ReferenceLevel;
    }
}
