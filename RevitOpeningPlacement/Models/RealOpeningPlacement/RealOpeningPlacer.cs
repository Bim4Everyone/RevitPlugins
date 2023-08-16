using System;
using System.Linq;

using dosymep.Revit;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Extensions;

namespace RevitOpeningPlacement.Models.RealOpeningPlacement {
    internal class RealOpeningPlacer {
        private readonly RevitRepository _revitRepository;


        public RealOpeningPlacer(RevitRepository revitRepository) {
            if(revitRepository == null) { throw new ArgumentNullException(nameof(revitRepository)); }
            _revitRepository = revitRepository;
        }


        internal void Place() {
            var host = _revitRepository.PickHostForRealOpening();
            var openingTasks = _revitRepository.PickOpeningTasksIncoming().Where(opening => opening.IntersectsSolid(host.GetSolid(), host.GetBoundingBox()));


        }
    }
}
