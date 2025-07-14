using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitFinishing.Models.Finishing;
internal class FinishingInProject {
    private readonly IReadOnlyCollection<FinishingElement> _allFinishing;

    public FinishingInProject(RevitRepository revitRepository, Phase phase) {
        IEnumerable<FinishingElement> walls = revitRepository
            .GetFinishingElementsOnPhase(FinishingCategory.Walls, phase);
        IEnumerable<FinishingElement> floors = revitRepository
            .GetFinishingElementsOnPhase(FinishingCategory.Floors, phase);
        IEnumerable<FinishingElement> ceilings = revitRepository
            .GetFinishingElementsOnPhase(FinishingCategory.Ceilings, phase);
        IEnumerable<FinishingElement> baseboards = revitRepository
            .GetFinishingElementsOnPhase(FinishingCategory.Baseboards, phase);

        _allFinishing = walls
            .Concat(baseboards)
            .Concat(ceilings)
            .Concat(floors)
            .ToList();
    }

    public IReadOnlyCollection<FinishingElement> AllFinishing => _allFinishing;
}
