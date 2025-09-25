using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitLintelPlacement.Models.Interfaces;

namespace RevitLintelPlacement.Models.LintelsProviders;

internal class ViewLintelsProvider : ILintelsProvider {
    private readonly View _currentView;
    private readonly RevitRepository _revitRepository;

    public ViewLintelsProvider(RevitRepository revitRepository, View currentView) {
        _revitRepository = revitRepository;
        _currentView = currentView;
    }

    public ICollection<FamilyInstance> GetLintels() {
        return _revitRepository.GetLintels(_revitRepository.GetViewElementCollector(_currentView));
    }
}
