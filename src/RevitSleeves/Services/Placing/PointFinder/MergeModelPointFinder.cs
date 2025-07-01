using Autodesk.Revit.DB;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.PointFinder;
internal class MergeModelPointFinder : IPointFinder<SleeveMergeModel> {
    public XYZ GetPoint(SleeveMergeModel param) {
        (var start, var end) = param.GetEndPoints();
        return (start + end) / 2;
    }
}
