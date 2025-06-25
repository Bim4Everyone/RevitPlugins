using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.PointFinder;
internal class PipeOpeningPointFinder : IPointFinder<ClashModel<Pipe, FamilyInstance>> {
    public XYZ GetPoint(ClashModel<Pipe, FamilyInstance> param) {
        return ((LocationCurve) param.MepElement.Location).Curve.GetEndPoint(0); // TODO
    }
}
