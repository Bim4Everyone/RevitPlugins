using dosymep.SimpleServices;

using RevitServerFolders.Models;
using RevitServerFolders.Services;

namespace RevitServerFolders.ViewModels {
    internal sealed class RsViewModel : MainViewModel {
        private readonly RsModelObjectConfig _pluginConfig;

        public RsViewModel(RsModelObjectConfig pluginConfig,
            IModelObjectService objectService,
            IOpenFolderDialogService openFolderDialogService)
            : base(pluginConfig, objectService, openFolderDialogService) {
            _pluginConfig = pluginConfig;
            IsExportRoomsVisible = false;
        }

        protected override void AcceptViewImpl() {
            base.AcceptViewImpl();
        }
    }
}
