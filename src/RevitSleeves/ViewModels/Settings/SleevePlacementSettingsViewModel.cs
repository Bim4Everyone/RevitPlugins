using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using pyRevitLabs.Json;

using RevitClashDetective.Models.FilterModel;

using RevitSleeves.Models;
using RevitSleeves.Models.Config;
using RevitSleeves.Services.Settings;
using RevitSleeves.ViewModels.Filtration;

namespace RevitSleeves.ViewModels.Settings;

internal class SleevePlacementSettingsViewModel : BaseViewModel {
    private readonly SleevePlacementSettingsConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly IConfigSerializer _configSerializer;
    private readonly IFilterChecker _filterChecker;
    private MepCategoryViewModel _activeMepCategorySettings;
    private string _errorText;
    private bool _showPlacingErrors;
    private string _name;

    public SleevePlacementSettingsViewModel(
        SleevePlacementSettingsConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        IMessageBoxService messageBoxService,
        ISaveFileDialogService saveFileDialogService,
        IOpenFileDialogService openFileDialogService,
        IFilterChecker filterChecker) {

        _pluginConfig = pluginConfig
            ?? throw new ArgumentNullException(nameof(pluginConfig));
        _revitRepository = revitRepository
            ?? throw new ArgumentNullException(nameof(revitRepository));
        _localizationService = localizationService
            ?? throw new ArgumentNullException(nameof(localizationService));
        _configSerializer = _pluginConfig.Serializer;
        MessageBoxService = messageBoxService
            ?? throw new ArgumentNullException(nameof(messageBoxService));
        SaveFileDialogService = saveFileDialogService
            ?? throw new ArgumentNullException(nameof(saveFileDialogService));
        OpenFileDialogService = openFileDialogService
            ?? throw new ArgumentNullException(nameof(openFileDialogService));
        _filterChecker = filterChecker
            ?? throw new ArgumentNullException(nameof(filterChecker));

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        SaveSettingsCommand = RelayCommand.Create(SaveConfig, CanAcceptView);
        SaveAsSettingsCommand = RelayCommand.Create(SaveAsConfig, CanAcceptView);
        ShowSettingsFolderCommand = RelayCommand.Create(ShowSettingsFolder);
        LoadSettingsCommand = RelayCommand.Create(LoadConfig);
        ShowMepFilterCommand = RelayCommand.Create(ShowMepFilter, CanAcceptView);
        ShowWallsFilterCommand = RelayCommand.Create(ShowWallsFilter, CanAcceptView);
        ShowFloorFilterCommand = RelayCommand.Create(ShowFloorsFilter, CanAcceptView);
    }


    public IMessageBoxService MessageBoxService { get; }

    public ISaveFileDialogService SaveFileDialogService { get; }

    public IOpenFileDialogService OpenFileDialogService { get; }

    public ICommand LoadViewCommand { get; }

    public ICommand AcceptViewCommand { get; }

    public ICommand SaveSettingsCommand { get; }

    public ICommand SaveAsSettingsCommand { get; }

    public ICommand ShowSettingsFolderCommand { get; }

    public ICommand LoadSettingsCommand { get; }

    public ICommand ShowMepFilterCommand { get; }

    public ICommand ShowWallsFilterCommand { get; }

    public ICommand ShowFloorFilterCommand { get; }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public bool ShowPlacingErrors {
        get => _showPlacingErrors;
        set => RaiseAndSetIfChanged(ref _showPlacingErrors, value);
    }

    public MepCategoryViewModel PipeSettings {
        get => _activeMepCategorySettings;
        set => RaiseAndSetIfChanged(ref _activeMepCategorySettings, value);
    }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }


    private void LoadView() {
        MapConfigToViewModel(_pluginConfig);
    }

    private void AcceptView() {
        SaveConfig();
    }

    private bool CanAcceptView() {
        if(string.IsNullOrWhiteSpace(Name)) {
            ErrorText = _localizationService.GetLocalizedString("SleevePlacementSettings.Validation.SetName");
            return false;
        }
        if(Name.Length > 100) {
            ErrorText = _localizationService.GetLocalizedString("SleevePlacementSettings.Validation.NameTooLong");
            return false;
        }
        string pipeError = PipeSettings.GetErrorText();
        if(!string.IsNullOrWhiteSpace(pipeError)) {
            ErrorText = pipeError;
            return false;
        }
        ErrorText = null;
        return true;
    }

    private void MapConfigToViewModel(SleevePlacementSettingsConfig config) {
        PipeSettings = new MepCategoryViewModel(
            _revitRepository,
            _localizationService,
            config.PipeSettings);
        Name = config.Name;
        ShowPlacingErrors = config.ShowPlacingErrors;
    }

    private void SaveConfig() {
        var config = GetCurrentSettings();
        config.SaveProjectConfig();
        SaveOpeningConfigPath(config.ProjectConfigPath);
    }

    private void SaveAsConfig() {
        var config = GetCurrentSettings();
        if(SaveFileDialogService.ShowDialog(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            _localizationService.GetLocalizedString("SleevePlacementSettings.DefaultFileName"))) {
            config.ProjectConfigPath = SaveFileDialogService.File.FullName;
            config.SaveProjectConfig();
            SaveOpeningConfigPath(config.ProjectConfigPath);
        }
    }

    private void LoadConfig() {
        if(OpenFileDialogService.ShowDialog()) {
            string path = OpenFileDialogService.File.FullName;
            string errorMsg = _localizationService.GetLocalizedString("Errors.CannotLoadPlacementSettings");
            try {
                var config = _configSerializer.Deserialize<SleevePlacementSettingsConfig>(File.ReadAllText(path));
                if(config is null) {
                    ShowErrorMessage(errorMsg);
                    return;
                }
                MapConfigToViewModel(config);
            } catch(JsonException) {
                ShowErrorMessage(errorMsg);
            }
        }
    }

    private void ShowSettingsFolder() {
        string path = GetCurrentSettings().ProjectConfigPath;
        const string explorer = "explorer.exe";
        const string selectMask = "/select,\"{0}\"";
        if(File.Exists(path)) {
            Process.Start(explorer, string.Format(selectMask, Path.GetFullPath(path)));
        } else {
            Process.Start(Path.GetFullPath(Path.GetDirectoryName(path)));
        }
    }

    private SleevePlacementSettingsConfig GetCurrentSettings() {
        var config = SleevePlacementSettingsConfig.GetPluginConfig(_configSerializer);
        config.Name = Name;
        config.ShowPlacingErrors = ShowPlacingErrors;
        config.PipeSettings = PipeSettings.GetSettings<PipeSettings>();
        return config;
    }

    private void SaveOpeningConfigPath(string path) {
        var configPath = SleevePlacementSettingsConfigPath.GetPluginConfig(_configSerializer);
        configPath.Path = path;
        configPath.SaveProjectConfig();
    }

    private void ShowErrorMessage(string message) {
        MessageBoxService.Show(
            message,
            _localizationService.GetLocalizedString("Error"),
            System.Windows.MessageBoxButton.OK);
    }

    private void ShowMepFilter() {
        ShowFilter(PipeSettings.MepFilterViewModel);
    }

    private void ShowWallsFilter() {
        ShowFilter(PipeSettings.WallSettings.StructureFilterViewModel);
    }

    private void ShowFloorsFilter() {
        ShowFilter(PipeSettings.FloorSettings.StructureFilterViewModel);
    }

    private void ShowFilter(SetViewModel setViewModel) {
        SaveConfig();
        var filter = new Filter(_revitRepository.GetClashRevitRepository()) {
            CategoryIds = [setViewModel.CategoryInfo.Category.Id],
            Name = setViewModel.CategoryInfo.Name,
            Set = setViewModel.GetSet()
        };
        _filterChecker.ShowFilter(filter);
    }
}
