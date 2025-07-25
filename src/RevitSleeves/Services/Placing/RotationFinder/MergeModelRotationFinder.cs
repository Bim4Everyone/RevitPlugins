using Autodesk.Revit.DB;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.RotationFinder;
internal class MergeModelRotationFinder : IRotationFinder<SleeveMergeModel> {
    public Rotation GetRotation(SleeveMergeModel param) {
        var orientation = param.GetOrientation();
        double angle = XYZ.BasisX.AngleOnPlaneTo(orientation, XYZ.BasisZ);
        return new Rotation() { AngleOZ = angle };
    }
}
