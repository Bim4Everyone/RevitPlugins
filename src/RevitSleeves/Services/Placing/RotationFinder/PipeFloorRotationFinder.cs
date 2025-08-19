using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.RotationFinder;
internal class PipeFloorRotationFinder : IRotationFinder<ClashModel<Pipe, Floor>> {
    public Rotation GetRotation(ClashModel<Pipe, Floor> param) {
        return new Rotation();
    }
}
