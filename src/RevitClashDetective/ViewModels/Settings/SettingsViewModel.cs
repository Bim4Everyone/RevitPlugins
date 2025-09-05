using System;
using System.Collections.Generic;
using System.Linq;
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
    private SectionBoxModeViewModel _selectedSectionBoxMode;
    private int _sectionBoxOffset;
    private const int _maxSectionBoxOffset = 10000;

    public SettingsViewModel(RevitRepository revitRepository, SettingsConfig config, ILocalizationService localizationService) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));

        MainElementVisibilitySettings = new(_localizationService);
        SecondElementVisibilitySettings = new(_localizationService);

        SectionBoxModes = [.. Enum.GetValues(typeof(SectionBoxMode))
                .Cast<SectionBoxMode>()
                .Select(w => new SectionBoxModeViewModel(w, _localizationService))];

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

    public IReadOnlyCollection<SectionBoxModeViewModel> SectionBoxModes { get; }

    public SectionBoxModeViewModel SelectedSectionBoxMode {
        get => _selectedSectionBoxMode;
        set => RaiseAndSetIfChanged(ref _selectedSectionBoxMode, value);
    }

    public int SectionBoxOffset {
        get => _sectionBoxOffset;
        set => RaiseAndSetIfChanged(ref _sectionBoxOffset, value);
    }


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
        if(SectionBoxOffset < 0) {
            ErrorText = _localizationService.GetLocalizedString(
                "SettingsWindow.Validation.OffsetLessThanZero");
            return false;
        }
        if(SectionBoxOffset > _maxSectionBoxOffset) {
            ErrorText = _localizationService.GetLocalizedString(
                "SettingsWindow.Validation.OffsetGreaterThan", _maxSectionBoxOffset);
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

        SectionBoxOffset = _config.SectionBoxOffset;
        SelectedSectionBoxMode = new SectionBoxModeViewModel(_config.SectionBoxModeSettings, _localizationService);
    }

    private void SaveConfig() {
        _config.MainElementVisibilitySettings = MainElementVisibilitySettings.GetSettings();
        _config.SecondElementVisibilitySettings = SecondElementVisibilitySettings.GetSettings();
        _config.SectionBoxOffset = SectionBoxOffset;
        _config.SectionBoxModeSettings = SelectedSectionBoxMode.Mode;
        _config.SaveProjectConfig();
    }
}
