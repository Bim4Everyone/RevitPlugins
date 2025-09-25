using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitLintelPlacement.Models.Interfaces;
using RevitLintelPlacement.ViewModels;

namespace RevitLintelPlacement.Models.ElementInWallProviders;

internal class SelectedElementsInWallProvider : IElementsInWallProvider {
    private readonly ElementInfosViewModel _elementInfos;
    private readonly RevitRepository _revitRepository;

    public SelectedElementsInWallProvider(RevitRepository revitRepository, ElementInfosViewModel elementInfos) {
        _revitRepository = revitRepository;
        _elementInfos = elementInfos;
    }

    public ICollection<FamilyInstance> GetElementsInWall() {
        return _revitRepository.GetElementsInWall(
            _revitRepository.GetSelectedElementsCollector(),
            _revitRepository.GetSelectedElementsCollector(),
            _elementInfos);
    }
}
