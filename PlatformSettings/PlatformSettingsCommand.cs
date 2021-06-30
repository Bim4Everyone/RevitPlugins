#region Namespaces
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
}
