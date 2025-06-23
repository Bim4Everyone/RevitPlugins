using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.LevelFinder;
internal class PipeOpeningLevelFinder : ILevelFinder<ClashModel<Pipe, FamilyInstance>> {
    public ClashModel<Pipe, FamilyInstance> GetLevel(ClashModel<Pipe, FamilyInstance> param) {
        throw new NotImplementedException();
    }
}
