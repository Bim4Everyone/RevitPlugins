using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitLintelPlacement.Models.Interfaces;

namespace RevitLintelPlacement.Models.LintelsProviders {
    internal class CurrentViewLintelsProvider : ILintelsProvider {
        private readonly RevitRepository _revitRepository;

        public CurrentViewLintelsProvider(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        public ICollection<FamilyInstance> GetLintels() {
            return _revitRepository.GetLintels(_revitRepository.GetViewElementCollector(_revitRepository.GetCurrentView()));
        }
    }
}