#region Namespaces
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;

using PlatformSettings.TabExtensions;

using pyRevitLabs.NLog;
using pyRevitLabs.PyRevit;

#endregion

namespace PlatformSettings {
    [Transaction(TransactionMode.Manual)]
    public class PlatformSettingsCommand : BasePluginCommand {
        public PlatformSettingsCommand() {
            PluginName = "Настройки платформы";
        }

        protected override void Execute(UIApplication uiApplication) {
            OpenSettingsWindow(uiApplication);
        }

        public bool OpenSettingsWindow(UIApplication uiApplication) {
            var window = new SettingsWindow() {DataContext = new PlatformSettingsViewModel()};
            var helper = new WindowInteropHelper(window) {Owner = uiApplication.MainWindowHandle};

            if(window.ShowDialog() == true) {
                var settings = (PlatformSettingsViewModel) window.DataContext;
                settings.SaveSettings();

                return true;
            }

            return false;
        }
    }
}
