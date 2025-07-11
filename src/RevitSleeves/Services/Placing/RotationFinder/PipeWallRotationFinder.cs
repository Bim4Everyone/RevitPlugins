using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.RotationFinder;
internal class PipeWallRotationFinder : IRotationFinder<ClashModel<Pipe, Wall>> {
    public Rotation GetRotation(ClashModel<Pipe, Wall> param) {
        var pipeDir = ((Line) ((LocationCurve) param.MepElement.Location).Curve).Direction;
        double angle = XYZ.BasisX.AngleOnPlaneTo(pipeDir, XYZ.BasisZ);
        return new Rotation() { AngleOZ = angle };
    }
}
