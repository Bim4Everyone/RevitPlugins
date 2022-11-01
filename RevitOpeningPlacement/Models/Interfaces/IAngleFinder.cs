using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.OpeningPlacement.AngleFinders;

namespace RevitOpeningPlacement.Models.Interfaces {
    internal interface IAngleFinder {
        Rotates GetAngle();
    }
}
