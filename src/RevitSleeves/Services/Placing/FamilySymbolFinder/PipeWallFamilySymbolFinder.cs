using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.FamilySymbolFinder;
internal class PipeWallFamilySymbolFinder : FamilySymbolFinder, IFamilySymbolFinder<ClashModel<Pipe, Wall>> {
    public FamilySymbol GetFamilySymbol(ClashModel<Pipe, Wall> param) {
        return GetSleeveFamilySymbol(param.MepElement.Document);
    }
}
