using System;
using System.ComponentModel;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;

/// <summary>
/// Настройки статусов Навигатора для файла КР
/// </summary>
internal class KrNavigatorSettingsViewModel : BaseViewModel {
    public KrNavigatorSettingsViewModel(
        OpeningRealsKrConfig openingRealsKrConfig,
        ILocalizationService localization) {
        if(openingRealsKrConfig is null) {
            throw new ArgumentNullException(nameof(openingRealsKrConfig));
        }

        var settings = openingRealsKrConfig.NavigatorSettings;
        IncomingStatuses = new IncomingStatusesViewModel(settings.IncomingTaskSettings);
        RealOpeningStatuses = new RealOpeningKrStatusesViewModel(settings.RealOpeningSettings, localization);
    }

    /// <summary>
    /// Статусы входящих заданий на отверстия
    /// </summary>
    public IncomingStatusesViewModel IncomingStatuses { get; }

    /// <summary>
    /// Статусы чистовых отверстий КР
    /// </summary>
    public RealOpeningKrStatusesViewModel RealOpeningStatuses { get; }

    /// <summary>
    /// Записывает текущие настройки статусов Навигатора в конфиг
    /// </summary>
    public void UpdateConfig(OpeningRealsKrConfig config) {
        var settings = config.NavigatorSettings;
        IncomingStatuses.UpdateConfig(settings.IncomingTaskSettings);
        RealOpeningStatuses.UpdateConfig(settings.RealOpeningSettings);
    }
}
