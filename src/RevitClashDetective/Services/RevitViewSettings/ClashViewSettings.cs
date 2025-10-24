using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Interfaces;
using RevitClashDetective.ViewModels.Navigator;

namespace RevitClashDetective.Services.RevitViewSettings;
internal class ClashViewSettings : IView3DSetting {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localization;
    private readonly IClashViewModel _clashModel;
    private readonly SettingsConfig _config;

    public ClashViewSettings(
        RevitRepository revitRepository,
        ILocalizationService localization,
        IClashViewModel clashModel,
        SettingsConfig config) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _clashModel = clashModel ?? throw new ArgumentNullException(nameof(clashModel));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public void Apply(View3D view3D) {
        var settings = GetSettings().ToArray();
        foreach(var setting in settings) {
            setting.Apply(view3D);
        }
    }

    private IEnumerable<IView3DSetting> GetSettings() {
        if(_config.ApplySectionBoxSettings) {
            yield return new BboxViewSettings(_revitRepository, _clashModel.GetElements(), _config);
        }
        if(_config.ApplyIsolationSettings || _config.ApplyColorSettings) {
            // перед изоляцией и перед раскраской элементов коллизии обязательно надо очистить фильтры, но только 1 раз
            yield return new EmptyFiltersViewSettings(_revitRepository, _localization);
        }
        if(_config.ApplyIsolationSettings) {
            yield return new ClashIsolationViewSettings(_revitRepository, _localization, _clashModel);
        }
        if(_config.ApplyColorSettings) {
            yield return new ColorClashViewSettings(_revitRepository, _localization, _clashModel, _config);
        }
    }
}
