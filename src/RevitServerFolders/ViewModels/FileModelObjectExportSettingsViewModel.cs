using dosymep.SimpleServices;

using RevitServerFolders.Models;
using RevitServerFolders.Services;

namespace RevitServerFolders.ViewModels;

internal class FileModelObjectExportSettingsViewModel : ExportSettingsViewModel<FileModelObjectExportSettings> {
    public FileModelObjectExportSettingsViewModel(FileModelObjectExportSettings settings,
        IModelObjectService objectService,
        IOpenFolderDialogService openFolderDialogService,
        ILocalizationService localization)
        : base(settings, objectService, openFolderDialogService, localization) {

        IsExportRooms = _settings.IsExportRooms;
        IsExportRoomsVisible = true;
    }


    public override FileModelObjectExportSettings GetSettings() {
        var settings = base.GetSettings();
        settings.IsExportRooms = IsExportRooms;
        return settings;
    }
}
