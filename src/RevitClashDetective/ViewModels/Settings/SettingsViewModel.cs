using System;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;

namespace RevitClashDetective.ViewModels.Settings;
internal class SettingsViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly SettingsConfig _config;
    private readonly ILocalizationService _localizationService;
    private string _errorText;

    public SettingsViewModel(RevitRepository revitRepository, SettingsConfig config, ILocalizationService localizationService) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));

        MainElementVisibilitySettings = new(_localizationService);
        SecondElementVisibilitySettings = new(_localizationService);

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }


    public ICommand LoadViewCommand { get; }

    public ICommand AcceptViewCommand { get; }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public ElementVisibilitySettingsViewModel MainElementVisibilitySettings { get; }

    public ElementVisibilitySettingsViewModel SecondElementVisibilitySettings { get; }


    private void LoadView() {
        LoadConfig();
    }

    private void AcceptView() {
        SaveConfig();
    }

    private bool CanAcceptView() {
        if(MainElementVisibilitySettings.Transparency < 0 || SecondElementVisibilitySettings.Transparency < 0) {
            ErrorText = _localizationService.GetLocalizedString(
                "SettingsWindow.Validation.TransparancyLessThanZero");
            return false;
        }
        if(MainElementVisibilitySettings.Transparency > 100 || SecondElementVisibilitySettings.Transparency > 100) {
            ErrorText = _localizationService.GetLocalizedString(
                "SettingsWindow.Validation.TransparancyGreaterThanHundred");
            return false;
        }

        ErrorText = null;
        return true;
    }

    private void LoadConfig() {
        MainElementVisibilitySettings.Color = _config.MainElementVisibilitySettings.Color;
        MainElementVisibilitySettings.Transparency = _config.MainElementVisibilitySettings.Transparency;

        SecondElementVisibilitySettings.Color = _config.SecondElementVisibilitySettings.Color;
        SecondElementVisibilitySettings.Transparency = _config.SecondElementVisibilitySettings.Transparency;
    }

    private void SaveConfig() {
        _config.MainElementVisibilitySettings = MainElementVisibilitySettings.GetSettings();
        _config.SecondElementVisibilitySettings = SecondElementVisibilitySettings.GetSettings();
        _config.SaveProjectConfig();
    }
}
