using Autodesk.Revit.DB;

namespace RevitSleeves.Services.Placing.FamilySymbolFinder;
internal interface IFamilySymbolFinder<T> where T : class {
    FamilySymbol GetFamilySymbol(T param);
}
