using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.FamilySymbolFinder;
internal class PipeFloorFamilySymbolFinder : FamilySymbolFinder, IFamilySymbolFinder<ClashModel<Pipe, Floor>> {
    public FamilySymbol GetFamilySymbol(ClashModel<Pipe, Floor> param) {
        return GetSleeveFamilySymbol(param.MepElement.Document);
    }
}
