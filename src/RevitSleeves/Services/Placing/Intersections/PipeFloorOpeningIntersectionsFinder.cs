using System;
using System.Collections.Generic;
using System.Threading;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models;
using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Core;

namespace RevitSleeves.Services.Placing.Intersections;
internal class PipeFloorOpeningIntersectionsFinder : MepOpeningCollisionFinder, IClashFinder<Pipe, FamilyInstance> {
    public PipeFloorOpeningIntersectionsFinder(
        RevitRepository revitRepository,
        IOpeningGeometryProvider openingGeometryProvider)
        : base(revitRepository, openingGeometryProvider) {
    }

    public ICollection<ClashModel<Pipe, FamilyInstance>> FindClashes(IProgress<int> progress, CancellationToken ct) {
        throw new NotImplementedException();
    }
}
