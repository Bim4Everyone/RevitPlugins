#region Namespaces
using PlatformSettings.TabExtensions;

#endregion

namespace PlatformSettings.TabExtensions {
    /// <summary>
    /// Включает и выключает расширение.
    /// </summary>
    internal class DisableExtensionBehav : IToggleExtension {
        private readonly PyRevitExtensionViewModel _viewModel;

        /// <summary>
        /// Создает экземпляр класс <see cref="DisableExtensionBehav"/>
        /// </summary>
        /// <param name="viewModel">Расширение которое переключают</param>
        public DisableExtensionBehav(PyRevitExtensionViewModel viewModel) {
            _viewModel = viewModel;
        }

        /// <inheritdoc/>
        public void Toggle(bool enabled) {
            if(_viewModel.InstalledExtension != null) {
                PyRevitExtensionsEx.ToggleExtension(_viewModel.InstalledExtension, enabled);
            }
        }
    }
}
