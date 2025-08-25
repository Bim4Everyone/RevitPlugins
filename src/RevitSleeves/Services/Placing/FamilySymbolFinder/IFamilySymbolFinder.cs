using Autodesk.Revit.DB;

namespace RevitSleeves.Services.Placing.FamilySymbolFinder;
internal interface IFamilySymbolFinder {
    FamilySymbol GetFamilySymbol();
}
