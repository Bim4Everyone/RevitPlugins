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

            if(!isChecked) {
                throw new OperationCanceledException();
            }
            
            var viewModel = new RoomsViewModel(new RevitRepository(uiApplication));
            var window = new RoomsWindow() { Title = PluginName, DataContext = viewModel };
            Notification(window);
        }
    }
}
