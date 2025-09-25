using Autodesk.Revit.DB;

namespace RevitLintelPlacement.Models.Interfaces;

internal interface IChecker {
    IResultHandler Check(FamilyInstance lintel, FamilyInstance elementInWall);
}
