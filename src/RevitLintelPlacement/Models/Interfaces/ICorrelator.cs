using Autodesk.Revit.DB;

namespace RevitLintelPlacement.Models.Interfaces;

internal interface ICorrelator {
    FamilyInstance Correlate(FamilyInstance lintel);
}
