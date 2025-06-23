using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.FamilySymbolFinder;
internal class PipeWallFamilySymbolFinder : IFamilySymbolFinder<ClashModel<Pipe, Wall>> {
    public FamilySymbol GetFamilySymbol(ClashModel<Pipe, Wall> param) {
        throw new NotImplementedException();
    }
}
