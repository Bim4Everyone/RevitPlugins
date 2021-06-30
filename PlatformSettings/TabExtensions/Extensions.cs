#region Namespaces
using System.Collections.Generic;
using System.Linq;

using PlatformSettings.TabExtensions;

using pyRevitLabs.PyRevit;

#endregion

namespace PlatformSettings.TabExtensions {
    internal abstract class Extensions {
        public abstract string Path { get; }
        public abstract List<string> Url { get; }

        protected abstract PyRevitExtensionViewModel GetPyRevitExtensionViewModel(PyRevitExtensionDefinition extension);

        public List<PyRevitExtensionViewModel> GetPyRevitExtensionViewModels() {
            List<PyRevitExtension> installedExtensions = GetInstallExtensions();
            List<PyRevitExtensionDefinition> extensions = GetPyRevitExtensionDefinitions();

            return extensions.Select(extension => {
                var viewModel = GetPyRevitExtensionViewModel(extension);

                var installedExtension = installedExtensions.FirstOrDefault(item => item.Name.Equals(extension.Name));
                viewModel.InstalledExtension = installedExtension;
                viewModel.Enabled = PyRevitExtensionsEx.IsEnabledExtension(extension.GetExtensionName());

                return viewModel;
            }).ToList();
        }

        private List<PyRevitExtension> GetInstallExtensions() {
            return PyRevitExtensions.GetInstalledExtensions(Path);
        }

        private List<PyRevitExtensionDefinition> GetPyRevitExtensionDefinitions() {
            return Url.SelectMany(url => PyRevitExtensionsEx.LookupExtensionInDefinitionFile(url, null)).ToList();
        }
    }
}
