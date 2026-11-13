using System;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;

/// <summary>
/// Вью модель окна настроек раздела АР
/// </summary>
internal class ArSettingsViewModel : BaseViewModel {
    private readonly OpeningRealsArConfig _config;
    private readonly ILocalizationService _localization;
    private readonly ArPlacementSettingsViewModel _placementVm;
    private readonly ArNavigatorSettingsViewModel _navigatorVm;
    private string _errorText;

    public ArSettingsViewModel(
        OpeningRealsArConfig config,
        ILocalizationService localization,
        ArPlacementSettingsViewModel placementVm,
        ArNavigatorSettingsViewModel navigatorVm) {
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
        if(!_navigatorVm.RealOpeningArStatuses.CanAcceptViewCommand.CanExecute(null)
           || !string.IsNullOrEmpty(_navigatorVm.RealOpeningArStatuses.ErrorText)) {
            string title = _localization.GetLocalizedString("SettingsNavigation.NavigatorSettings");
            ErrorText = $"{title}: {_navigatorVm.RealOpeningArStatuses.ErrorText}";
            return false;
        }

        ErrorText = null;
        return true;
    }
}
