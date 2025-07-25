using System;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Services.RevitViewSettings;

internal class EmptyFiltersViewSettings : IView3DSetting {
    private readonly RevitRepository _revitRepository;

    public EmptyFiltersViewSettings(RevitRepository revitRepository) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
    }


    public void Apply(View3D view3D) {
        ClearViewFilters(view3D);
    }

    private void ClearViewFilters(View3D view) {
        using var t = _revitRepository.Doc.StartTransaction("Сброс фильтров элементов коллизии");
        _revitRepository.RemoveFilters(view);
        t.Commit();
    }
}
