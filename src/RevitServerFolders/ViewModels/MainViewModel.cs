using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitServerFolders.Models;
using RevitServerFolders.Services;

namespace RevitServerFolders.ViewModels;
internal class MainViewModel<T> : BaseViewModel where T : ExportSettings {
    private readonly IModelsExportService<T> _exportService;
    protected readonly PluginConfig<T> _pluginConfig;
    protected readonly IModelObjectService _objectService;
    protected readonly ILocalizationService _localization;

    private string _errorText;
    private ExportSettingsViewModel<T> _selectedSettings;
    private bool _allSelected;

    public MainViewModel(PluginConfig<T> pluginConfig,
        IModelsExportService<T> exportService,
        IModelObjectService objectService,
        IOpenFolderDialogService openFolderDialogService,
        IProgressDialogFactory progressDialogFactory,
        ILocalizationService localization) {
        _pluginConfig = pluginConfig
            ?? throw new ArgumentNullException(nameof(pluginConfig));
        _exportService = exportService
            ?? throw new ArgumentNullException(nameof(exportService));
        _objectService = objectService
            ?? throw new ArgumentNullException(nameof(objectService));
        OpenFolderDialogService = openFolderDialogService
            ?? throw new ArgumentNullException(nameof(openFolderDialogService));
        ProgressDialogFactory = progressDialogFactory
            ?? throw new ArgumentNullException(nameof(progressDialogFactory));
        _localization = localization
            ?? throw new ArgumentNullException(nameof(localization));
        SettingsCollection = [];

        LoadViewCommand = RelayCommand.CreateAsync(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        AddSettingsCommand = RelayCommand.Create(AddSettings);
        RemoveSettingsCommand = RelayCommand.Create<ExportSettingsViewModel<T>>(RemoveSettings, CanRemoveSettings);
    }

    public string Title { get; protected set; }

    public IAsyncCommand LoadViewCommand { get; }

    public ICommand AcceptViewCommand { get; }

    public ICommand AddSettingsCommand { get; }

    public ICommand RemoveSettingsCommand { get; }

    public IOpenFolderDialogService OpenFolderDialogService { get; }

    public IProgressDialogFactory ProgressDialogFactory { get; }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public ExportSettingsViewModel<T> SelectedSettings {
        get => _selectedSettings;
        set => RaiseAndSetIfChanged(ref _selectedSettings, value);
    }

    public bool AllSelected {
        get => _allSelected;
        set {
            if(_allSelected != value) {
                RaiseAndSetIfChanged(ref _allSelected, value);
                foreach(var item in SettingsCollection) {
                    item.IsSelected = value;
                }
            }
        }
    }

    public ObservableCollection<ExportSettingsViewModel<T>> SettingsCollection { get; }


    protected virtual void LoadConfigImpl() { }

    protected virtual void AddSettingsImpl() { }

    private void UpdateIndexes() {
        int index = 1;
        foreach(var item in SettingsCollection) {
            item.Index = index++;
        }
    }

    private async Task LoadView() {
        LoadConfig();
        foreach(var setting in SettingsCollection) {
            await setting.SourceFolderChangedCommand.ExecuteAsync(default);
        }
    }

    private void AcceptView() {
        SaveConfig();
        var selectedSettings = SettingsCollection
            .Where(s => s.IsSelected)
            .ToArray();

        using var dialog = ProgressDialogFactory.CreateDialog();
        dialog.StepValue = 1;
        dialog.MaxValue = selectedSettings
            .SelectMany(GetModelObjects)
            .Count();
        var progress = dialog.CreateProgress();
        var ct = dialog.CreateCancellationToken();
        dialog.Show();

        int startProcess = 0;
        foreach(var item in selectedSettings) {
            ExportModelObjects(item, progress, ct, startProcess);
            startProcess += GetModelObjects(item).Length;
        }
    }

    private void ExportModelObjects(ExportSettingsViewModel<T> settingsViewModel,
        IProgress<int> progress,
        CancellationToken ct = default,
        int processStart = 0) {

        string[] modelFiles = GetModelObjects(settingsViewModel);

        _exportService.ExportModelObjects(modelFiles, settingsViewModel.GetSettings(), progress, ct, processStart);

        if(settingsViewModel.OpenTargetWhenFinish) {
            Process.Start(Path.GetFullPath(settingsViewModel.TargetFolder));
        }
    }

    private string[] GetModelObjects(ExportSettingsViewModel<T> settings) {
        return settings.ModelObjects
            .Where(item => !item.SkipObject)
            .Select(item => item.FullName)
            .ToArray();
    }

    private bool CanAcceptView() {
        var errorSettings = SettingsCollection
            .Where(s => s.IsSelected)
            .Select(s => new { Index = s.Index, Error = s.GetErrorText() })
            .FirstOrDefault(a => !string.IsNullOrWhiteSpace(a.Error));
        if(errorSettings is not null) {
            ErrorText = _localization.GetLocalizedString(
                "MainWindow.ErrorTextPattern", errorSettings.Index, errorSettings.Error);
            return false;
        }
        if(SettingsCollection.All(s => !s.IsSelected)) {
            ErrorText = _localization.GetLocalizedString("MainWindow.Validation.AllSettingsOff");
            return false;
        }

        ErrorText = null;
        return true;
    }

    private void LoadConfig() {
        LoadConfigImpl();
        SelectedSettings = SettingsCollection.FirstOrDefault(s => s.IsSelected) ?? SettingsCollection.First();
        UpdateIndexes();
    }

    private void SaveConfig() {
        _pluginConfig.ExportSettings = [.. SettingsCollection.Select(s => s.GetSettings())];
        _pluginConfig.SaveProjectConfig();
    }

    private void AddSettings() {
        AddSettingsImpl();
        UpdateIndexes();
    }

    private void RemoveSettings(ExportSettingsViewModel<T> settings) {
        SettingsCollection.Remove(settings);
        UpdateIndexes();
        SelectedSettings = SettingsCollection.FirstOrDefault();
    }

    private bool CanRemoveSettings(ExportSettingsViewModel<T> settings) {
        return settings is not null && SettingsCollection.Count > 1;
    }
}
