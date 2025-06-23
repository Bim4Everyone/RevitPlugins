using System;
using System.Collections.Generic;
using System.Threading;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.Intersections;
internal class PipeOpeningIntersectionsFinder : IClashFinder<Pipe, Floor> {
    public ICollection<ClashModel<Pipe, Floor>> FindClashes(IProgress<int> progress, CancellationToken ct) {
        throw new NotImplementedException();
    }
}
