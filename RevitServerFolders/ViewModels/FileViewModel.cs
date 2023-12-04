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
        }

        protected override void LoadConfigImpl() {
            base.LoadConfigImpl();
        }

        protected override void SaveConfigImpl() {
            base.SaveConfigImpl();
        }

        protected override void AcceptViewImpl() {
            base.AcceptViewImpl();
        }
    }
}
