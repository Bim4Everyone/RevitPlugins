#region Namespaces

using System.Collections.Generic;
using System.Linq;

using pyRevitLabs.PyRevit;

#endregion

namespace PlatformSettings.Legacy.TabExtensions {
    /// <summary>
    /// Класс получения расширений.
    /// </summary>
    internal abstract class Extensions {
        /// <summary>
        /// Путь до папки с расширениями.
        /// </summary>
        public abstract string Path { get; }

        /// <summary>
        /// Список URL метаданных расширений.
        /// </summary>
        public abstract List<string> Url { get; }

        /// <summary>
        /// Возвращает объект расширения для View.
        /// </summary>
        /// <param name="extension">Определение расширения.</param>
        /// <returns>Возвращает объект расширения для View.</returns>
        protected abstract PyRevitExtensionViewModel GetPyRevitExtensionViewModel(PyRevitExtensionDefinitionEx extension);

        /// <summary>
        /// Возвращает список всех расширений для View.
        /// </summary>
        /// <returns>Возвращает список всех расширений для View.</returns>
        public List<PyRevitExtensionViewModel> GetPyRevitExtensionViewModels() {
            List<PyRevitExtension> installedExtensions = GetInstallExtensions();
            List<PyRevitExtensionDefinitionEx> extensions = GetPyRevitExtensionDefinitions();

            return extensions.Select(extension => {
                var viewModel = GetPyRevitExtensionViewModel(extension);

                var installedExtension = installedExtensions.FirstOrDefault(item => item.Name.Equals(extension.Name));
                viewModel.InstalledExtension = installedExtension;
                viewModel.Enabled = PyRevitExtensionsEx.IsEnabledExtension(extension.GetExtensionName());

                return viewModel;
            }).ToList();
        }

        /// <summary>
        /// Возвращает список установленных расширений.
        /// </summary>
        /// <returns>Возвращает список установленных расширений.</returns>
        private List<PyRevitExtension> GetInstallExtensions() {
            return PyRevitExtensions.GetInstalledExtensions(Path);
        }

        /// <summary>
        /// Возвращает определения расширений по списку URL.
        /// </summary>
        /// <returns>Возвращает определения расширений по списку URL.</returns>
        private List<PyRevitExtensionDefinitionEx> GetPyRevitExtensionDefinitions() {
            return Url.SelectMany(url => PyRevitExtensionsEx.LookupExtensionInDefinitionFile(url, null)).ToList();
        }
    }
}
