using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.PlacingOptsFinder;
internal class PipeOpeningPlacingOptsProvider : ISleevePlacingOptsProvider<ClashModel<Pipe, FamilyInstance>> {
    public SleevePlacingOpts GetOpts(ClashModel<Pipe, FamilyInstance> param) {
        throw new NotImplementedException();
    }
}
