using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;
using dosymep.Bim4Everyone;

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
                var viewModel = new RoomsViewModel(uiApplication.Application, uiApplication.ActiveUIDocument.Document);
                var window = new RoomsWindow() {DataContext = viewModel};
                var helper = new WindowInteropHelper(window) {Owner = uiApplication.MainWindowHandle};

                window.ShowDialog();
            }
        }
    }
}
