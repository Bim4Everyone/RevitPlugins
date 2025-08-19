using System.Linq;

using Autodesk.Revit.DB;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.LevelFinder;
internal class MergeModelLevelFinder : ILevelFinder<SleeveMergeModel> {
    public Level GetLevel(SleeveMergeModel param) {
        var doc = param.Document;
        return (Level) doc.GetElement(param.GetSleeves().First().GetFamilyInstance().LevelId);
    }
}
