using dosymep.SimpleServices;

using RevitServerFolders.Models;
using RevitServerFolders.Services;

namespace RevitServerFolders.ViewModels;

internal class RsModelObjectExportSettingsViewModel : ExportSettingsViewModel<RsModelObjectExportSettings> {
    public RsModelObjectExportSettingsViewModel(RsModelObjectExportSettings settings,
        IModelObjectService objectService,
        IOpenFolderDialogService openFolderDialogService,
        ILocalizationService localization)
        : base(settings, objectService, openFolderDialogService, localization) {

        IsExportRoomsVisible = false;
    }
}
