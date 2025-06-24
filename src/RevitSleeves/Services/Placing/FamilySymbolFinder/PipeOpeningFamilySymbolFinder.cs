using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.FamilySymbolFinder;
internal class PipeOpeningFamilySymbolFinder
    : FamilySymbolFinder, IFamilySymbolFinder<ClashModel<Pipe, FamilyInstance>> {
    public FamilySymbol GetFamilySymbol(ClashModel<Pipe, FamilyInstance> param) {
        return GetSleeveFamilySymbol(param.MepElement.Document);
    }
}
