using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitLintelPlacement.Models.Interfaces;
using RevitLintelPlacement.ViewModels;

namespace RevitLintelPlacement.Models.LintelsProviders {
    internal class AllLintelsProvider : ILintelsProvider {
        private readonly RevitRepository _revitRepository;

        public AllLintelsProvider(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        public ICollection<FamilyInstance> GetLintels() {
            return _revitRepository.GetLintels(_revitRepository.GetAllElementsCollector());
        }
    }
}