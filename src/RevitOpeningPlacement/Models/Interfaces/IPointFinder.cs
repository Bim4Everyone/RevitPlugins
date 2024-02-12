using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Interfaces {
    internal interface IPointFinder {
        XYZ GetPoint();
    }
}
