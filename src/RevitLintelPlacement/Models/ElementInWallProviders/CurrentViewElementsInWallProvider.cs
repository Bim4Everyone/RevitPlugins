using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitLintelPlacement.Models.Interfaces;
using RevitLintelPlacement.ViewModels;

namespace RevitLintelPlacement.Models.ElementInWallProviders {
    internal class CurrentViewElementsInWallProvider : IElementsInWallProvider {
        private readonly RevitRepository _revitRepository;
        private readonly ElementInfosViewModel _elementInfos;
        private readonly View _currentView;

        public CurrentViewElementsInWallProvider(RevitRepository revitRepository, ElementInfosViewModel elementInfos, View currentView) {
            _revitRepository = revitRepository;
            _elementInfos = elementInfos;
            _currentView = currentView;
        }

        public ICollection<FamilyInstance> GetElementsInWall() {
            return _revitRepository.GetElementsInWall(_revitRepository.GetViewElementCollector(_currentView), _revitRepository.GetViewElementCollector(_currentView), _elementInfos);
        }
    }
}
