#region Namespaces
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using PlatformSettings.TabExtensions;

using pyRevitLabs.NLog;
using pyRevitLabs.PyRevit;

#endregion

namespace PlatformSettings {
    [Transaction(TransactionMode.Manual)]
    public class PlatformSettingsCommand : IExternalCommand {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            return Execute(commandData) ? Result.Succeeded : Result.Cancelled;
        }

        public bool Execute(ExternalCommandData commandData) {
            var bimExtension = new BimExtensions();
            var pyExtension = new PyExtensions();

            var extensions = bimExtension.GetPyRevitExtensionViewModels().Union(pyExtension.GetPyRevitExtensionViewModels());
            var settings = new PlatformSettingsViewModel() { PyRevitExtensions = new ObservableCollection<PyRevitExtensionViewModel>(extensions) };

            var window = new SettingsWindow() { DataContext = settings };
            new WindowInteropHelper(window) { Owner = commandData.Application.MainWindowHandle };

            if(window.ShowDialog() == true) {
                foreach(var extension in settings.PyRevitExtensions) {
                    extension.ToggleExtension.Toggle(extension.Enabled);
                }

                return true;
            }

            return false;
        }
    }

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

    internal class BimExtensions : Extensions {
        public override string Path { get; } = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "pyRevit", "Extensions");

        public override List<string> Url { get; } = new List<string>() {
            "https://raw.githubusercontent.com/dosymep/BIMExtensions/master/extensions.json"
        };


        protected override PyRevitExtensionViewModel GetPyRevitExtensionViewModel(PyRevitExtensionDefinition extension) {
            var viewModel = new PyRevitExtensionViewModel(extension) { AllowChangeEnabled = !extension.BuiltIn };
            viewModel.ToggleExtension = new UnistalExtensionBehav(viewModel);

            if(viewModel.BuiltIn) {
                PyRevitExtensionsEx.ToggleExtension(viewModel.ExtensionName, viewModel.BuiltIn);
            }

            return viewModel;
        }
    }

    internal class PyExtensions : Extensions {
        public override string Path { get; } = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "pyRevit-Master", "Extensions");

        public override List<string> Url { get; } = new List<string>() {
            "https://raw.githubusercontent.com/eirannejad/pyRevit/master/extensions/pyRevitTags.extension/extension.json",
            "https://raw.githubusercontent.com/eirannejad/pyRevit/master/extensions/pyRevitTools.extension/extension.json",
        };

        protected override PyRevitExtensionViewModel GetPyRevitExtensionViewModel(PyRevitExtensionDefinition extension) {
            var viewModel = new PyRevitExtensionViewModel(extension) { AllowChangeEnabled = true };
            viewModel.ToggleExtension = new DisableExtensionBehav(viewModel);

            return viewModel;
        }
    }

    public interface IToggleExtension {
        void Toggle(bool enabled);
    }

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
