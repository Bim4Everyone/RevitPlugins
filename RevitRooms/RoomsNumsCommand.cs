using System;
using System.Collections.Generic;
using System.Linq;
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
    public class RoomsNumsCommand : BasePluginCommand {
        public RoomsNumsCommand() {
            PluginName = "Нумерация помещений с приоритетом";
        }

        protected override void Execute(UIApplication uiApplication) {
            var isChecked = new CheckProjectParams(uiApplication)
                .CopyProjectParams()
                .CopyKeySchedules()
                .CheckKeySchedules()
                .GetIsChecked();

            if(!isChecked) {
                throw new OperationCanceledException();
            }

            var window = new RoomsNumsWindows() {Title = PluginName};
            window.DataContext = new RoomNumsViewModel(new RevitRepository(uiApplication), window);
            Notification(window);
        }
    }
}