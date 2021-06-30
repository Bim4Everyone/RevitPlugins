#region Namespaces
using PlatformSettings.TabExtensions;

#endregion

namespace PlatformSettings.TabExtensions {
    internal class DisableExtensionBehav : IToggleExtension {
        private readonly PyRevitExtensionViewModel _viewModel;

        public DisableExtensionBehav(PyRevitExtensionViewModel viewModel) {
            _viewModel = viewModel;
        }

        public void Toggle(bool enabled) {
            if(_viewModel.InstalledExtension != null) {
                PyRevitExtensionsEx.ToggleExtension(_viewModel.InstalledExtension, enabled);
            }
        }
    }
}
