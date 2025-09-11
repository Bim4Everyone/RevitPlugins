using System;
using System.Linq;

using dosymep.SimpleServices;

using RevitServerFolders.Models;
using RevitServerFolders.Services;

namespace RevitServerFolders.ViewModels;
internal sealed class RsViewModel : MainViewModel<RsModelObjectExportSettings> {
    public RsViewModel(RsModelObjectConfig pluginConfig,
        IModelObjectService objectService,
        IModelsExportService<RsModelObjectExportSettings> exportService,
        IOpenFolderDialogService openFolderDialogService,
        IProgressDialogFactory progressDialogFactory,
        ILocalizationService localization)
        : base(pluginConfig,
            exportService,
            objectService,
            openFolderDialogService,
            progressDialogFactory,
            localization) {

        Title = _localization.GetLocalizedString("RvtFromRsWindow.Title");
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
        .Select(s => new RsModelObjectExportSettingsViewModel(s,
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

    private RsModelObjectExportSettingsViewModel CreateSetting() {
        return new RsModelObjectExportSettingsViewModel(
            new RsModelObjectExportSettings(), _objectService, OpenFolderDialogService, _localization);
    }
}
