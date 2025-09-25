using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitLintelPlacement.Models.Interfaces;
using RevitLintelPlacement.ViewModels;

namespace RevitLintelPlacement.Models.ElementInWallProviders;

internal class AllElementsInWallProvider : IElementsInWallProvider {
    private readonly ElementInfosViewModel _elementInfos;
    private readonly RevitRepository _revitRepository;

    public AllElementsInWallProvider(RevitRepository revitRepository, ElementInfosViewModel elementInfos) {
        _revitRepository = revitRepository;
        _elementInfos = elementInfos;
    }

    public ICollection<FamilyInstance> GetElementsInWall() {
        return _revitRepository.GetElementsInWall(
            _revitRepository.GetAllElementsCollector(),
            _revitRepository.GetAllElementsCollector(),
            _elementInfos);
    }
}
