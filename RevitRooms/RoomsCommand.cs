using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;

using RevitRooms.Models;
using RevitRooms.ViewModels;
using RevitRooms.Views;

namespace RevitRooms {
    [Transaction(TransactionMode.Manual)]
    public class RoomsCommand : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            AppDomain.CurrentDomain.AssemblyResolve += AppDomainExtensions.CurrentDomain_AssemblyResolve;
            try {
                var isChecked = new CheckProjectParams(commandData.Application)
                    .CopyProjectParams()
                    .CopyKeySchedules()
                    .CheckKeySchedules()
                    .GetIsChecked();

                if(!isChecked) {
                    TaskDialog.Show("Квартирография Стадии П.", "Заполните атрибуты у квартир.");
                    return Result.Succeeded;
                }

                var viewModel = new RoomsViewModel(commandData.Application.Application, commandData.Application.ActiveUIDocument.Document);
                var window = new RoomsWindow() { DataContext = viewModel };
                new WindowInteropHelper(window) { Owner = commandData.Application.MainWindowHandle };

                window.ShowDialog();

            } catch(Exception ex) {
#if D2020 || D2021 || D2022
                TaskDialog.Show("Квартирография Стадии П.", ex.ToString());
#else
                TaskDialog.Show("Квартирография Стадии П.", ex.Message);
#endif
            } finally {
                AppDomain.CurrentDomain.AssemblyResolve -= AppDomainExtensions.CurrentDomain_AssemblyResolve;
            }

            return Result.Succeeded;
        }
    }
}
