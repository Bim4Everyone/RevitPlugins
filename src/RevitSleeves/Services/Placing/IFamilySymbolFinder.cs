using Autodesk.Revit.DB;

namespace RevitSleeves.Services.Placing;
internal interface IFamilySymbolFinder<T> where T : class {
    FamilySymbol GetFamilySymbol(T param);
}
