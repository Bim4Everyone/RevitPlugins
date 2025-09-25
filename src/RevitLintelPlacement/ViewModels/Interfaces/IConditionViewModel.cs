using Autodesk.Revit.DB;

namespace RevitLintelPlacement.ViewModels.Interfaces;

internal interface IConditionViewModel {
    bool Check(FamilyInstance elementInWall);
}
