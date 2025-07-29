using System;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Services.RevitViewSettings;

internal class EmptyFiltersViewSettings : IView3DSetting {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    public EmptyFiltersViewSettings(RevitRepository revitRepository, ILocalizationService localizationService) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
    }


    public void Apply(View3D view3D) {
        ClearViewFilters(view3D);
    }

    private void ClearViewFilters(View3D view) {
        using var t = _revitRepository.Doc.StartTransaction(
            _localizationService.GetLocalizedString("Transactions.ClearFilters"));
        _revitRepository.RemoveFilters(view);
        t.Commit();
    }
}
