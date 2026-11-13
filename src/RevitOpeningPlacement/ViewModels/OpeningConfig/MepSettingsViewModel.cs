using System;
using System.ComponentModel;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;

/// <summary>
/// Вью модель окна настроек раздела ВИС
/// </summary>
internal class MepSettingsViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localization;
    private readonly MepPlacementSettingsViewModel _placementVm;
    private readonly MepNavigatorSettingsViewModel _navigatorVm;
    private string _errorText;

    public MepSettingsViewModel(
        RevitRepository revitRepository,
        ILocalizationService localization,
        MepPlacementSettingsViewModel placementVm,
        MepNavigatorSettingsViewModel navigatorVm) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _placementVm = placementVm ?? throw new ArgumentNullException(nameof(placementVm));
        _navigatorVm = navigatorVm ?? throw new ArgumentNullException(nameof(navigatorVm));

        SaveConfigCommand = RelayCommand.Create(SaveConfig, CanSaveConfig);
    }

    public ICommand SaveConfigCommand { get; }

    public IOpenFileDialogService OpenFileDialogService => _placementVm.OpenFileDialogService;

    public ISaveFileDialogService SaveFileDialogService => _placementVm.SaveFileDialogService;

    public IMessageBoxService MessageBoxService => _placementVm.MessageBoxService;

    public string ErrorText {
        get => _errorText;
        private set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    private void SaveConfig() {
        var config = Models.Configs.OpeningConfig.GetOpeningConfig(_revitRepository.Doc);
        _placementVm.UpdateConfig(config);
        _navigatorVm.UpdateConfig(config);
        config.SaveProjectConfig();
    }

    private bool CanSaveConfig() {
        if(!string.IsNullOrEmpty(_placementVm.ErrorText)) {
            string title = _localization.GetLocalizedString("SettingsNavigation.PlacementSettings");
            ErrorText = $"{title}: {_placementVm.ErrorText}";
            return false;
        }

        ErrorText = null;
        return true;
    }
}
