using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    private ParamViewModel _selectedParam;
    private const int _maxSectionBoxOffset = 10000;
    private const int _maxParamNameLength = 128;

    public SettingsViewModel(RevitRepository revitRepository, SettingsConfig config, ILocalizationService localizationService) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));

        MainElementVisibilitySettings = new(_localizationService);
        SecondElementVisibilitySettings = new(_localizationService);

        SectionBoxModes = [.. Enum.GetValues(typeof(SectionBoxMode))
                .Cast<SectionBoxMode>()
                .Select(w => new SectionBoxModeViewModel(w, _localizationService))];
        Params = [];

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        AddParamCommand = RelayCommand.Create(AddParam);
        RemoveParamCommand = RelayCommand.Create<ParamViewModel>(RemoveParam, CanRemoveParam);
        MoveParamUpCommand = RelayCommand.Create<ParamViewModel>(MoveParamUp, CanMoveParamUp);
        MoveParamDownCommand = RelayCommand.Create<ParamViewModel>(MoveParamDown, CanMoveParamDown);
    }


    public ICommand LoadViewCommand { get; }

    public ICommand AcceptViewCommand { get; }

    public ICommand MoveParamUpCommand { get; }

    public ICommand MoveParamDownCommand { get; }

    public ICommand AddParamCommand { get; }

    public ICommand RemoveParamCommand { get; }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public ElementVisibilitySettingsViewModel MainElementVisibilitySettings { get; }

    public ElementVisibilitySettingsViewModel SecondElementVisibilitySettings { get; }

    public IReadOnlyCollection<SectionBoxModeViewModel> SectionBoxModes { get; }

    public ObservableCollection<ParamViewModel> Params { get; }

    public ParamViewModel SelectedParam {
        get => _selectedParam;
        set => RaiseAndSetIfChanged(ref _selectedParam, value);
    }

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
        if(Params.Any(p => string.IsNullOrWhiteSpace(p.Name))) {
            ErrorText = _localizationService.GetLocalizedString(
                "SettingsWindow.Validation.ParamIsEmpty");
            return false;
        }
        var longParam = Params.FirstOrDefault(p => p.Name.Length > _maxParamNameLength);
        if(longParam is not null) {
            ErrorText = _localizationService.GetLocalizedString(
                "SettingsWindow.Validation.LongParam", longParam.Name.Substring(0, 16) + "...");
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

        Array.ForEach(_config.ParamNames, p => Params.Add(new ParamViewModel() { Name = p }));
    }

    private void SaveConfig() {
        _config.MainElementVisibilitySettings = MainElementVisibilitySettings.GetSettings();
        _config.SecondElementVisibilitySettings = SecondElementVisibilitySettings.GetSettings();
        _config.SectionBoxOffset = SectionBoxOffset;
        _config.SectionBoxModeSettings = SelectedSectionBoxMode.Mode;
        _config.ParamNames = [.. Params.Select(p => p.Name.Trim())];
        _config.SaveProjectConfig();
    }

    private void MoveParamUp(ParamViewModel param) {
        int indexFrom = Params.IndexOf(param);
        int indexTo = indexFrom - 1;
        (Params[indexFrom], Params[indexTo]) = (Params[indexTo], Params[indexFrom]);
        SelectedParam = Params[indexTo];
    }

    private bool CanMoveParamUp(ParamViewModel param) {
        return param is not null
            && Params.IndexOf(param) > 0;
    }

    private void MoveParamDown(ParamViewModel param) {
        int indexFrom = Params.IndexOf(param);
        int indexTo = indexFrom + 1;
        (Params[indexFrom], Params[indexTo]) = (Params[indexTo], Params[indexFrom]);
        SelectedParam = Params[indexTo];
    }

    private bool CanMoveParamDown(ParamViewModel param) {
        return param is not null
            && (Params.IndexOf(param) < (Params.Count - 1));
    }

    private void AddParam() {
        var p = new ParamViewModel();
        SelectedParam = p;
        Params.Add(p);
    }

    private void RemoveParam(ParamViewModel param) {
        Params.Remove(param);
        SelectedParam = Params.FirstOrDefault();
    }

    private bool CanRemoveParam(ParamViewModel param) {
        return param is not null;
    }
}
