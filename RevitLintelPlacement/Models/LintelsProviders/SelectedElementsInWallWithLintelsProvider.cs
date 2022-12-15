using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitLintelPlacement.Models.Interfaces;
using RevitLintelPlacement.ViewModels;

namespace RevitLintelPlacement.Models.LintelsProviders {
    internal class SelectedElementsInWallWithLintelsProvider : ILintelsProvider {
        private readonly RevitRepository _revitRepository;
        private readonly IElementsInWallProvider _elementsInWallProvider;

        public SelectedElementsInWallWithLintelsProvider(RevitRepository revitRepository, IElementsInWallProvider elementsInWallProvider) {
            _revitRepository = revitRepository;
            _elementsInWallProvider = elementsInWallProvider;
        }

        public ICollection<FamilyInstance> GetLintels() {
            var lintels = _revitRepository.GetLintels(_revitRepository.GetAllElementsCollector());
            var correlator = new DimensionCorrelator(_revitRepository);
            var elementsInWall = _elementsInWallProvider.GetElementsInWall();
            return lintels.Select(item => new {
                Lintel = item,
                Opening = correlator.Correlate(item)
            })
                .Where(item => elementsInWall.Any(o => o.Id == item.Opening?.Id))
                .Select(item => item.Lintel)
                .ToArray();
        }
    }
}