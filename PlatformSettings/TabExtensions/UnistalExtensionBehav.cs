#region Namespaces
using PlatformSettings.TabExtensions;

using pyRevitLabs.PyRevit;

#endregion

namespace PlatformSettings {
    internal class UnistalExtensionBehav : IToggleExtension {
        private readonly PyRevitExtensionViewModel _viewModel;

        public UnistalExtensionBehav(PyRevitExtensionViewModel viewModel) {
            _viewModel = viewModel;
        }

        public void Toggle(bool enabled) {
            if(enabled) {
                if(_viewModel.InstalledExtension == null) {
                    PyRevitExtensions.InstallExtension(_viewModel.Name, _viewModel.Type, _viewModel.Url);
                    PyRevitExtensionsEx.ToggleExtension(_viewModel.ExtensionName, enabled);
                }
            } else {
                if(_viewModel.InstalledExtension != null) {
                    PyRevitExtensions.UninstallExtension(_viewModel.InstalledExtension);
                    PyRevitExtensionsEx.ToggleExtension(_viewModel.ExtensionName, enabled);
                }
            }
        }
    }
}
