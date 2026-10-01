using System;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;

/// <summary>
/// Вью модель окна настроек раздела КР
/// </summary>
internal class KrSettingsViewModel : BaseViewModel {
    private readonly OpeningRealsKrConfig _config;
    private readonly ILocalizationService _localization;
    private readonly KrPlacementSettingsViewModel _placementVm;
    private readonly KrNavigatorSettingsViewModel _navigatorVm;
    private string _errorText;

    public KrSettingsViewModel(
        OpeningRealsKrConfig config,
        ILocalizationService localization,
        KrPlacementSettingsViewModel placementVm,
        KrNavigatorSettingsViewModel navigatorVm) {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _placementVm = placementVm ?? throw new ArgumentNullException(nameof(placementVm));
        _navigatorVm = navigatorVm ?? throw new ArgumentNullException(nameof(navigatorVm));

        SaveConfigCommand = RelayCommand.Create(SaveConfig, CanSaveConfig);
    }

    public ICommand SaveConfigCommand { get; }

    public string ErrorText {
        get => _errorText;
        private set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    private void SaveConfig() {
        _placementVm.UpdateConfig(_config);
        _navigatorVm.UpdateConfig(_config);
        _config.SaveProjectConfig();
    }

    private bool CanSaveConfig() {
        if(!_navigatorVm.RealOpeningStatuses.CanAcceptViewCommand.CanExecute(null)
           || !string.IsNullOrEmpty(_navigatorVm.RealOpeningStatuses.ErrorText)) {
            string title = _localization.GetLocalizedString("SettingsNavigation.NavigatorSettings");
            ErrorText = $"{title}: {_navigatorVm.RealOpeningStatuses.ErrorText}";
            return false;
        }

        ErrorText = null;
        return true;
    }
}
