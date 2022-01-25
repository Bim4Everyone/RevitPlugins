using System;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;

using RevitLintelPlacement.ViewModels;
using RevitLintelPlacement.Views;

namespace RevitLintelPlacement {

    [Transaction(TransactionMode.Manual)]
    public class PlaceLintelCommand : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            AppDomain.CurrentDomain.AssemblyResolve += AppDomainExtensions.CurrentDomain_AssemblyResolve;
            try {
                var mainViewModel = new MainViewModel();
                var window = new MainWindow() { DataContext = mainViewModel };
                new WindowInteropHelper(window) { Owner = commandData.Application.MainWindowHandle };

                window.ShowDialog();
            } catch(Exception ex) {
#if D2020 || D2021 || D2022
                TaskDialog.Show("Расстановщик перемычек.", ex.ToString());
#else
                TaskDialog.Show("Расстановщик перемычек.", ex.Message);
#endif
            } finally {
                AppDomain.CurrentDomain.AssemblyResolve -= AppDomainExtensions.CurrentDomain_AssemblyResolve;
            }

            return Result.Succeeded;
        }
    }
}
