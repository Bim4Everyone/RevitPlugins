using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitFinishing.Models.Finishing
{
    internal class FinishingInProject {
        private readonly IReadOnlyCollection<Element> _walls;
        private readonly IReadOnlyCollection<Element> _floors;
        private readonly IReadOnlyCollection<Element> _ceilings;
        private readonly IReadOnlyCollection<Element> _baseboards;
        private readonly IReadOnlyCollection<Element> _allFinishing;

        public FinishingInProject(RevitRepository revitRepository, Phase phase) {
            _walls = revitRepository.GetFinishingElementsOnPhase(FinishingCategory.Walls, phase);
            _floors = revitRepository.GetFinishingElementsOnPhase(FinishingCategory.Floors, phase);
            _ceilings = revitRepository.GetFinishingElementsOnPhase(FinishingCategory.Ceilings, phase);
            _baseboards = revitRepository.GetFinishingElementsOnPhase(FinishingCategory.Baseboards, phase);

            _allFinishing = _walls
                .Concat(_baseboards)
                .Concat(_ceilings)
                .Concat(_floors)
                .ToList();
        }
        public IReadOnlyCollection<Element> Walls => _walls;
        public IReadOnlyCollection<Element> Floors => _floors;
        public IReadOnlyCollection<Element> Ceilings => _ceilings;
        public IReadOnlyCollection<Element> Baseboards => _baseboards;
        public IReadOnlyCollection<Element> AllFinishing => _allFinishing;
    }
}
