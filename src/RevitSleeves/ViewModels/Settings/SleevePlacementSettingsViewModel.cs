using System;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitSleeves.Models;
using RevitSleeves.Models.Config;

namespace RevitSleeves.ViewModels.Settings;

internal class SleevePlacementSettingsViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly ISaveFileDialogService _saveFileDialogService;
    private readonly IOpenFileDialogService _openFileDialogService;
    private MepCategoryViewModel _activeMepCategorySettings;
    private string _errorText;
    private bool _showPlacingErrors;

    public SleevePlacementSettingsViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        ISaveFileDialogService saveFileDialogService,
        IOpenFileDialogService openFileDialogService) {

        _pluginConfig = pluginConfig
            ?? throw new ArgumentNullException(nameof(pluginConfig));
        _revitRepository = revitRepository
            ?? throw new ArgumentNullException(nameof(revitRepository));
        _localizationService = localizationService
            ?? throw new ArgumentNullException(nameof(localizationService));
        _saveFileDialogService = saveFileDialogService
            ?? throw new ArgumentNullException(nameof(saveFileDialogService));
        _openFileDialogService = openFileDialogService
            ?? throw new ArgumentNullException(nameof(openFileDialogService));

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    public ICommand LoadViewCommand { get; }

    public ICommand AcceptViewCommand { get; }

    public ICommand SaveAsSettingsCommand { get; }

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

    public MepCategoryViewModel ActiveMepCategorySettings {
        get => _activeMepCategorySettings;
        set => RaiseAndSetIfChanged(ref _activeMepCategorySettings, value);
    }


    private void LoadView() {
        LoadConfig();
    }

    private void AcceptView() {
        SaveConfig();
    }

    private bool CanAcceptView() {
        //if(Math.Round(1.0) > 0) { TODO
        //    ErrorText = _localizationService.GetLocalizedString("MainWindow.HelloCheck");
        //    return false;
        //}

        ErrorText = null;
        return true;
    }

    private void LoadConfig() {
        ActiveMepCategorySettings = new MepCategoryViewModel(
            _revitRepository,
            _localizationService,
            _pluginConfig.PipeSettings);
    }

    private void SaveConfig() {
        _pluginConfig.SaveProjectConfig();
    }
}
