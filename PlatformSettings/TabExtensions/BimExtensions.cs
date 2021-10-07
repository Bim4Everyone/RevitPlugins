#region Namespaces
using System;
using System.Collections.Generic;

using PlatformSettings.TabExtensions;

using pyRevitLabs.PyRevit;

#endregion

namespace PlatformSettings.TabExtensions {
    /// <summary>
    /// Класс для получения расширений платформы.
    /// </summary>
    internal class BimExtensions : Extensions {
        /// <inheritdoc/>
        public override string Path { get; } = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "pyRevit", "Extensions");

        /// <inheritdoc/>
        public override List<string> Url { get; } = new List<string>() {
            System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"pyRevit\Extensions\01.BIM.extension\extensions.json")
        };

        /// <inheritdoc/>
        protected override PyRevitExtensionViewModel GetPyRevitExtensionViewModel(PyRevitExtensionDefinitionEx extension) {
            var viewModel = new PyRevitExtensionViewModel(extension) { AllowChangeEnabled = !extension.BuiltIn };
            viewModel.ToggleExtension = new UnistalExtensionBehav(viewModel);

            if(viewModel.BuiltIn) {
                PyRevitExtensionsEx.ToggleExtension(viewModel.ExtensionName, viewModel.BuiltIn);
            }

            return viewModel;
        }
    }
}
