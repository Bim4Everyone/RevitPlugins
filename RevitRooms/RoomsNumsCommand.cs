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

using RevitRooms.Models;
using RevitRooms.ViewModels;
using RevitRooms.Views;

namespace RevitRooms {
    [Transaction(TransactionMode.Manual)]
    public class RoomsNumsCommand : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            AppDomain.CurrentDomain.AssemblyResolve += AppDomainExtensions.CurrentDomain_AssemblyResolve;
            try {
                var isChecked = new CheckProjectParams(commandData.Application)
                    .CopyProjectParams()
                    .CopyKeySchedules()
                    .CheckKeySchedules()
                    .GetIsChecked();

                if(!isChecked) {
                    return Result.Succeeded;
                }

                var window = new RoomsNumsWindows();
                window.DataContext = new RoomNumsViewModel(commandData.Application.Application, commandData.Application.ActiveUIDocument.Document, window);
                
                new WindowInteropHelper(window) { Owner = commandData.Application.MainWindowHandle };
                window.ShowDialog();

            } catch(Exception ex) {
#if D2020 || D2021 || D2022
                TaskDialog.Show("Нумерация помещений с приоритетом.", ex.ToString());
#else
                TaskDialog.Show("Нумерация помещений с приоритетом.", ex.Message);
#endif
            } finally {
                AppDomain.CurrentDomain.AssemblyResolve -= AppDomainExtensions.CurrentDomain_AssemblyResolve;
            }

            return Result.Succeeded;
        }
    }
}
