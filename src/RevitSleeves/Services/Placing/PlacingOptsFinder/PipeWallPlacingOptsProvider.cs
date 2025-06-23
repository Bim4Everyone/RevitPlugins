using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.PlacingOptsFinder;
internal class PipeWallPlacingOptsProvider : ISleevePlacingOptsProvider<ClashModel<Pipe, Wall>> {

    public SleevePlacingOpts GetOpts(ClashModel<Pipe, Wall> param) {
        throw new NotImplementedException();
    }
}
