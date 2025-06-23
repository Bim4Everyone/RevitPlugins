using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.LevelFinder;
internal class PipeWallLevelFinder : ILevelFinder<ClashModel<Pipe, Wall>> {
    public ClashModel<Pipe, Wall> GetLevel(ClashModel<Pipe, Wall> param) {
        throw new NotImplementedException();
    }
}
