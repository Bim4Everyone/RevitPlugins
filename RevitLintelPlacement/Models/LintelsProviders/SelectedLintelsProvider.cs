using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitLintelPlacement.Models.Interfaces;

namespace RevitLintelPlacement.Models.LintelsProviders {
    internal class SelectedLintelsProvider : ILintelsProvider {
        private readonly RevitRepository _revitRepository;

        public SelectedLintelsProvider(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        public ICollection<FamilyInstance> GetLintels() {
            return _revitRepository.GetLintels(_revitRepository.GetSelectedElementsCollector());
        }
    }
}