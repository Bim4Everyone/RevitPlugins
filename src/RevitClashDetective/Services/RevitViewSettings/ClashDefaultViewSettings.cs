using System;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Services.RevitViewSettings;

internal class ClashDefaultViewSettings : IView3DSetting {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly ClashModel _clashModel;
    private readonly SettingsConfig _config;

    public ClashDefaultViewSettings(RevitRepository revitRepository,
        ILocalizationService localizationService,
        ClashModel clashModel,
        SettingsConfig config) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _clashModel = clashModel ?? throw new ArgumentNullException(nameof(clashModel));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }


    public void Apply(View3D view3D) {
        var bboxSettings = new BboxViewSettings(_revitRepository,
            [_clashModel.MainElement, _clashModel.OtherElement],
            10);
        var filterSettings = new EmptyFiltersViewSettings(_revitRepository, _localizationService);
        var colorSettings = new ColorClashViewSettings(_revitRepository, _localizationService, _clashModel, _config);
        bboxSettings.Apply(view3D);
        filterSettings.Apply(view3D);
        colorSettings.Apply(view3D);
    }
}
