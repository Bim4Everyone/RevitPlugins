using System;
using System.Linq;
using System.Threading.Tasks;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitServerFolders.Models;
using RevitServerFolders.Services;

namespace RevitServerFolders.ViewModels;
internal sealed class FileSystemViewModel : MainViewModel<FileModelObjectExportSettings> {
    private readonly INwcExportViewSettingsService _nwcExportViewSettingsService;

    public FileSystemViewModel(
        FileModelObjectConfig pluginConfig,
        IModelObjectService objectService,
        IModelsExportService<FileModelObjectExportSettings> exportService,
        IOpenFolderDialogService openFolderDialogService,
        IProgressDialogFactory progressDialogFactory,
        ILocalizationService localization,
        INwcExportViewSettingsService nwcExportViewSettingsService)
        : base(pluginConfig,
            exportService,
            objectService,
            openFolderDialogService,
            progressDialogFactory,
            localization) {

        SetNwcExportViewSettingsCommand = RelayCommand.CreateAsync(SetNwcExportViewSettings);

        Title = _localization.GetLocalizedString("NwcFromRvtWindow.Title");
        _nwcExportViewSettingsService = nwcExportViewSettingsService
            ?? throw new ArgumentNullException(nameof(nwcExportViewSettingsService));
    }


    public IAsyncCommand SetNwcExportViewSettingsCommand { get; }

    protected override void AddSettingsImpl() {
        var newSetting = CreateSetting();
        if(OpenFolderDialogService.ShowDialog()) {
            newSetting.TargetFolder = OpenFolderDialogService.Folder.FullName;
        } else {
            throw new OperationCanceledException();
        }
        SettingsCollection.Add(newSetting);
        SelectedSettings = newSetting;
    }

    protected override void LoadConfigImpl() {
        var settings = _pluginConfig.ExportSettings
            .Select(s => new FileModelObjectExportSettingsViewModel(s,
                _objectService,
                OpenFolderDialogService,
                _localization))
            .ToArray();
        if(settings.Length == 0) {
            SettingsCollection.Add(CreateSetting());
        } else {
            foreach(var setting in settings) {
                SettingsCollection.Add(setting);
            }
        }
    }

    private FileModelObjectExportSettingsViewModel CreateSetting() {
        return new FileModelObjectExportSettingsViewModel(
            new FileModelObjectExportSettings(), _objectService, OpenFolderDialogService, _localization);
    }

    protected override bool CanAcceptView() {
        var config = (FileModelObjectConfig) _pluginConfig;
        if(string.IsNullOrWhiteSpace(config.NwcExportViewSettings?.RvtFilePath)
            || string.IsNullOrWhiteSpace(config.NwcExportViewSettings?.ViewTemplateName)) {
            ErrorText = _localization.GetLocalizedString("NwcMainWindow.Validation.SetNwcViewSettings");
            return false;
        }

        return base.CanAcceptView();
    }

    protected override ExportSettingsViewModel<FileModelObjectExportSettings>[] GetSettingsForExport() {
        var settings = base.GetSettingsForExport()
            .Cast<FileModelObjectExportSettingsViewModel>()
            .ToArray();
        var config = (FileModelObjectConfig) _pluginConfig;
        foreach(var setting in settings) {
            setting.SetNwcExportViewSettings(config.NwcExportViewSettings);
        }
        return settings;
    }

    private async Task SetNwcExportViewSettings() {
        var config = (FileModelObjectConfig) _pluginConfig;
        var currentSettings = config.NwcExportViewSettings;
        var result = await _nwcExportViewSettingsService.ShowDialogAsync(config.NwcExportViewSettings);
        if(result.IsSuccess) {
            config.NwcExportViewSettings = result.Settings;
        }
    }
}
