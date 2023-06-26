using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;
using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

using RevitRooms.Models;
using RevitRooms.ViewModels;
using RevitRooms.Views;

namespace RevitRooms {
    [Transaction(TransactionMode.Manual)]
    public class RoomsCommand : BasePluginCommand {
        public RoomsCommand() {
            PluginName = "Квартирография Стадии П";
        }

        protected override void Execute(UIApplication uiApplication) {
            var isChecked = new CheckProjectParams(uiApplication)
                .CopyProjectParams()
                .CopyKeySchedules()
                .CheckKeySchedules()
                .GetIsChecked();

            if(isChecked) {
                var viewModel = new RoomsViewModel(new RevitRepository(uiApplication));
                var window = new RoomsWindow() { DataContext = viewModel };
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
}
