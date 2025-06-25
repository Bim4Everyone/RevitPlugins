using System.Linq;

using Autodesk.Revit.DB;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.PointFinder;
internal class MergeModelPointFinder : IPointFinder<SleeveMergeModel> {
    public XYZ GetPoint(SleeveMergeModel param) {
        return ((LocationPoint) param.GetSleeves().First().GetFamilyInstance().Location).Point; // TODO
    }
}
