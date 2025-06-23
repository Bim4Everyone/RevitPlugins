using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.PlacingOptsFinder;
internal class PipeFloorPlacingOptsProvider : IPlacingOptsProvider<ClashModel<Pipe, Floor>> {
    public SleevePlacingOpts GetOpts(ClashModel<Pipe, Floor> param) {
        throw new NotImplementedException();
    }
}
