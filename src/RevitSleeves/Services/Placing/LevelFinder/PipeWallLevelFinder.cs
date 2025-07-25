using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.LevelFinder;
internal class PipeWallLevelFinder : ILevelFinder<ClashModel<Pipe, Wall>> {
    public Level GetLevel(ClashModel<Pipe, Wall> param) {
        return param.MepElement.ReferenceLevel;
    }
}
