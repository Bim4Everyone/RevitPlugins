using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;
using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

using RevitCheckingLevels.Models;
using RevitCheckingLevels.ViewModels;
using RevitCheckingLevels.Views;

namespace RevitCheckingLevels {
    [Transaction(TransactionMode.Manual)]
    public class RevitCheckingLevelsCommand : BasePluginCommand {
        public RevitCheckingLevelsCommand() {
            PluginName = "Проверка уровней";
        }

        protected override void Execute(UIApplication uiApplication) {
            var window = new MainWindow() {
                Title = PluginName,
                DataContext = new MainViewModel(uiApplication)
            };

            if(window.ShowDialog() == true) {
                GetPlatformService<INotificationService>()
                    .CreateNotification(PluginName, "Выполнение скрипта завершено успешно.", "C#")
                    .ShowAsync();
            } else {
                GetPlatformService<INotificationService>()
                    .CreateWarningNotification(PluginName, "Выполнение скрипта отменено.")
                    .ShowAsync();
            }
        }
    }
}
