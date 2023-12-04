using dosymep.SimpleServices;

using RevitServerFolders.Models;
using RevitServerFolders.Services;

namespace RevitServerFolders.ViewModels {
    internal sealed class FileSystemViewModel : MainViewModel {
        private readonly FileModelObjectConfig _pluginConfig;

        public FileSystemViewModel(FileModelObjectConfig pluginConfig,
            IModelObjectService objectService,
            IOpenFolderDialogService openFolderDialogService)
            : base(pluginConfig, objectService, openFolderDialogService) {
            _pluginConfig = pluginConfig;
            IsExportRoomsVisible = true;
        }

        protected override void LoadConfigImpl() {
            IsExportRooms = _pluginConfig.IsExportRooms;
        }

        protected override void SaveConfigImpl() {
            _pluginConfig.IsExportRooms= IsExportRooms;
        }

        protected override void AcceptViewImpl() {
            base.AcceptViewImpl();
        }
    }
}
