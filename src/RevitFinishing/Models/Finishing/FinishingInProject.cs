using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitFinishing.Models.Finishing.Factories;

namespace RevitFinishing.Models.Finishing;
internal class FinishingInProject {
    private readonly IReadOnlyCollection<FinishingElement> _allFinishing;

    public FinishingInProject(RevitRepository revitRepository, Phase phase) {
        FinishingWallFactory wallFactory = new FinishingWallFactory();
        FinishingFloorFactory floorFactory = new FinishingFloorFactory();
        FinishingCeilingFactory ceilingFactory = new FinishingCeilingFactory();
        FinishingBaseboardFactory baseboardFactory = new FinishingBaseboardFactory();

        IEnumerable<FinishingElement> walls = revitRepository
            .GetFinishingElementsOnPhase(FinishingCategory.Walls, wallFactory, phase);
        IEnumerable<FinishingElement> floors = revitRepository
            .GetFinishingElementsOnPhase(FinishingCategory.Floors, floorFactory, phase);
        IEnumerable<FinishingElement> ceilings = revitRepository
            .GetFinishingElementsOnPhase(FinishingCategory.Ceilings, ceilingFactory, phase);
        IEnumerable<FinishingElement> baseboards = revitRepository
            .GetFinishingElementsOnPhase(FinishingCategory.Baseboards, baseboardFactory, phase);

        _allFinishing = walls
            .Concat(baseboards)
            .Concat(ceilings)
            .Concat(floors)
            .ToList();
    }

    public IReadOnlyCollection<FinishingElement> AllFinishing => _allFinishing;
}
