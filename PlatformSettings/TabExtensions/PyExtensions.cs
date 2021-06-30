#region Namespaces
using System;
using System.Collections.Generic;

using PlatformSettings.TabExtensions;

using pyRevitLabs.PyRevit;

#endregion

namespace PlatformSettings.TabExtensions {
    /// <summary>
    /// Класс для получения расширений pyRevit.
    /// </summary>
    internal class PyExtensions : Extensions {
        /// <inheritdoc/>
        public override string Path { get; } = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "pyRevit-Master", "Extensions");

        /// <inheritdoc/>
        public override List<string> Url { get; } = new List<string>() {
            "https://raw.githubusercontent.com/eirannejad/pyRevit/master/extensions/pyRevitTags.extension/extension.json",
            "https://raw.githubusercontent.com/eirannejad/pyRevit/master/extensions/pyRevitTools.extension/extension.json",
        };

        /// <inheritdoc/>
        protected override PyRevitExtensionViewModel GetPyRevitExtensionViewModel(PyRevitExtensionDefinitionEx extension) {
            var viewModel = new PyRevitExtensionViewModel(extension) { AllowChangeEnabled = true };
            viewModel.ToggleExtension = new DisableExtensionBehav(viewModel);

            return viewModel;
        }
    }
}
