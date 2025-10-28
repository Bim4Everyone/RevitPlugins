using Autodesk.Revit.DB;

namespace RevitMarkPlacement.Models;

internal interface IUnitProvider {
    Units GetUnits();
}
