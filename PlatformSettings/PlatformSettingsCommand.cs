#region Namespaces
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

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
            if(OpenSettingsWindow(uiApplication)) {
                GetPlatformService<INotificationService>()
                    .CreateNotification(PluginName, "Выполнение скрипта завершено успешно.", "C#")
                    .ShowAsync();
            } else {
                GetPlatformService<INotificationService>()
                    .CreateWarningNotification(PluginName, "Выполнение скрипта отменено.")
                    .ShowAsync();
            }
        }

        public bool OpenSettingsWindow(UIApplication uiApplication) {
            var window = new SettingsWindow() { DataContext = new PlatformSettingsViewModel() };
            if(window.ShowDialog() == true) {
                var settings = (PlatformSettingsViewModel) window.DataContext;
                settings.SaveSettings();

                return true;
            }

            return false;
        }
    }
}
