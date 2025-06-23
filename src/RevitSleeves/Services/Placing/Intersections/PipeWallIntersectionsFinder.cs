using System;
using System.Collections.Generic;
using System.Threading;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.Intersections;
internal class PipeWallIntersectionsFinder : IClashFinder<Pipe, Wall> {
    public ICollection<ClashModel<Pipe, Wall>> FindClashes(IProgress<int> progress, CancellationToken ct) {
        throw new NotImplementedException();
    }
}
