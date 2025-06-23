using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.FamilySymbolFinder;
internal class PipeOpeningFamilySymbolFinder : IFamilySymbolFinder<ClashModel<Pipe, FamilyInstance>> {
    public FamilySymbol GetFamilySymbol(ClashModel<Pipe, FamilyInstance> param) {
        throw new NotImplementedException();
    }
}
