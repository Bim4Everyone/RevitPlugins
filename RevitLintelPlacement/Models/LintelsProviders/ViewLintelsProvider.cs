using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitLintelPlacement.Models.Interfaces;
using RevitLintelPlacement.ViewModels;

namespace RevitLintelPlacement.Models.LintelsProviders {
    internal class ViewLintelsProvider : ILintelsProvider {
        private readonly RevitRepository _revitRepository;
        private readonly View _currentView;

        public ViewLintelsProvider(RevitRepository revitRepository, View currentView) {
            _revitRepository = revitRepository;
            _currentView = currentView;
        }

        public ICollection<FamilyInstance> GetLintels() {
            return _revitRepository.GetLintels(_revitRepository.GetViewElementCollector(_currentView));
        }
    }
}