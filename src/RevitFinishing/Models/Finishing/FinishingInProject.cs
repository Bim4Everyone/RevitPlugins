using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitFinishing.Models.Finishing;
internal class FinishingInProject {
    private readonly FinishingWallFactory _wallFactory;
    private readonly FinishingFloorFactory _floorFactory;
    private readonly FinishingCeilingFactory _ceilingFactory;
    private readonly FinishingBaseboardFactory _baseboardFactory;

    private IReadOnlyCollection<FinishingElement> _allFinishing = new List<FinishingElement>();

    public FinishingInProject(FinishingWallFactory wallFactory,
                              FinishingFloorFactory floorFactory,
                              FinishingCeilingFactory ceilingFactory,
                              FinishingBaseboardFactory baseboardFactory) {
        _wallFactory = wallFactory;
        _floorFactory = floorFactory;
        _ceilingFactory = ceilingFactory;
        _baseboardFactory = baseboardFactory;
    }

    public void CalculateAllFinishing(RevitRepository revitRepository, Phase phase) {
        IEnumerable<FinishingElement> walls = revitRepository
            .GetFinishingElementsOnPhase(FinishingCategory.Walls, _wallFactory, phase);
        IEnumerable<FinishingElement> floors = revitRepository
            .GetFinishingElementsOnPhase(FinishingCategory.Floors, _floorFactory, phase);
        IEnumerable<FinishingElement> ceilings = revitRepository
            .GetFinishingElementsOnPhase(FinishingCategory.Ceilings, _ceilingFactory, phase);
        IEnumerable<FinishingElement> baseboards = revitRepository
            .GetFinishingElementsOnPhase(FinishingCategory.Baseboards, _baseboardFactory, phase);

        _allFinishing = walls
            .Concat(baseboards)
            .Concat(ceilings)
            .Concat(floors)
            .ToList();
    }

    public IReadOnlyCollection<FinishingElement> AllFinishing => _allFinishing;
}
