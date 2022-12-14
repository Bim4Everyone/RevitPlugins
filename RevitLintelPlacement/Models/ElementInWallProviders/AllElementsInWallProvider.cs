using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitLintelPlacement.Models.Interfaces;
using RevitLintelPlacement.ViewModels;

namespace RevitLintelPlacement.Models.ElementInWallProviders {
    internal class AllElementsInWallProvider : IElementsInWallProvider {
        private readonly RevitRepository _revitRepository;
        private readonly ElementInfosViewModel _elementInfos;

        public AllElementsInWallProvider(RevitRepository revitRepository, ElementInfosViewModel elementInfos) {
            _revitRepository = revitRepository;
            _elementInfos = elementInfos;
        }

        public ICollection<FamilyInstance> GetElementsInWall() {
            return _revitRepository.GetElementsInWall(_revitRepository.GetAllElementsCollector(), _revitRepository.GetAllElementsCollector(), _elementInfos);
        }
    }
}
