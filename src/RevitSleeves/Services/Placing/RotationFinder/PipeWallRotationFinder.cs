using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.RotationFinder;
internal class PipeWallRotationFinder : IRotationFinder<ClashModel<Pipe, Wall>> {
    public Rotation GetRotation(ClashModel<Pipe, Wall> param) {
        return new Rotation(); // TODO
    }
}
