using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.PointFinder;
internal class PipeFloorPointFinder : IPointFinder<ClashModel<Pipe, Floor>> {
    public XYZ GetPoint(ClashModel<Pipe, Floor> param) {
        throw new NotImplementedException();
    }
}
