using Autodesk.Revit.DB;

namespace RevitLintelPlacement.ViewModels.Interfaces;

internal interface ILintelParameterViewModel {
    void SetTo(FamilyInstance lintel, FamilyInstance elementInWall);
}
