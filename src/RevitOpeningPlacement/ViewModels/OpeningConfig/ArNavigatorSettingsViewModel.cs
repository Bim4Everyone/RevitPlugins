using System;
using System.ComponentModel;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;

/// <summary>
/// Настройки статусов Навигатора для файла АР
/// </summary>
internal class ArNavigatorSettingsViewModel : BaseViewModel {
    public ArNavigatorSettingsViewModel(
        OpeningRealsArConfig openingRealsArConfig,
        ILocalizationService localization) {
        if(openingRealsArConfig is null) {
            throw new ArgumentNullException(nameof(openingRealsArConfig));
        }

        var settings = openingRealsArConfig.NavigatorSettings;
        IncomingStatuses = new IncomingStatusesViewModel(settings.IncomingTaskSettings);
        RealOpeningArStatuses = new RealOpeningArStatusesViewModel(settings.RealOpeningSettings, localization);
    }

    /// <summary>
    /// Статусы входящих заданий на отверстия
    /// </summary>
    public IncomingStatusesViewModel IncomingStatuses { get; }

    /// <summary>
    /// Статусы чистовых отверстий
    /// </summary>
    public RealOpeningArStatusesViewModel RealOpeningArStatuses { get; }

    /// <summary>
    /// Записывает текущие настройки статусов Навигатора в конфиг
    /// </summary>
    public void UpdateConfig(OpeningRealsArConfig config) {
        var settings = config.NavigatorSettings;
        IncomingStatuses.UpdateConfig(settings.IncomingTaskSettings);
        RealOpeningArStatuses.UpdateConfig(settings.RealOpeningSettings);
    }
}
