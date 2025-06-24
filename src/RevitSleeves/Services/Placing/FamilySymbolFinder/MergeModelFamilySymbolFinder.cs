using Autodesk.Revit.DB;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.FamilySymbolFinder;
internal class MergeModelFamilySymbolFinder : FamilySymbolFinder, IFamilySymbolFinder<SleeveMergeModel> {
    public FamilySymbol GetFamilySymbol(SleeveMergeModel param) {
        return GetSleeveFamilySymbol(param.Document);
    }
}
