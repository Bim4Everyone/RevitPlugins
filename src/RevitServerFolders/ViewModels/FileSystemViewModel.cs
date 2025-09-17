using System;
using System.Linq;

using dosymep.SimpleServices;

using RevitServerFolders.Models;
using RevitServerFolders.Services;

namespace RevitServerFolders.ViewModels;
internal sealed class FileSystemViewModel : MainViewModel<FileModelObjectExportSettings> {
    public FileSystemViewModel(
        FileModelObjectConfig pluginConfig,
        IModelObjectService objectService,
        IModelsExportService<FileModelObjectExportSettings> exportService,
        IOpenFolderDialogService openFolderDialogService,
        IProgressDialogFactory progressDialogFactory,
        ILocalizationService localization)
        : base(pluginConfig,
            exportService,
            objectService,
            openFolderDialogService,
            progressDialogFactory,
            localization) {

        Title = _localization.GetLocalizedString("NwcFromRvtWindow.Title");
    }


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
}
