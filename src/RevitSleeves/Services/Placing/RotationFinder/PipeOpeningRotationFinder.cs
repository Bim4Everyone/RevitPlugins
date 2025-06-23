using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.RotationFinder;
internal class PipeOpeningRotationFinder : IRotationFinder<ClashModel<Pipe, FamilyInstance>> {
    public Rotation GetRotation(ClashModel<Pipe, FamilyInstance> param) {
        throw new NotImplementedException();
    }
}
