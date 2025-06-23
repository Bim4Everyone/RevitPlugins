using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.PointFinder;
internal class PipeWallPointFinder : IPointFinder<ClashModel<Pipe, Wall>> {
    public XYZ GetPoint(ClashModel<Pipe, Wall> param) {
        throw new NotImplementedException();
    }
}
