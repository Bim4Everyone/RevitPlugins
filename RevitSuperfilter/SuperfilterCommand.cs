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

using RevitSuperfilter.ViewModels;
using RevitSuperfilter.Views;

namespace RevitSuperfilter {
    [Transaction(TransactionMode.Manual)]
    public class SuperfilterCommand : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            AppDomain.CurrentDomain.AssemblyResolve += AppDomainExtensions.CurrentDomain_AssemblyResolve;
            try {
                var viewModel = new SuperfilterViewModel(commandData.Application.Application, commandData.Application.ActiveUIDocument.Document);

                var window = new MainWindow() { DataContext = viewModel };
                new WindowInteropHelper(window) { Owner = commandData.Application.MainWindowHandle };

                window.ShowDialog();
            } catch(Exception ex) {
#if D2020 || D2021 || D2022
                TaskDialog.Show("Суперфильтр.", ex.ToString());
#else
                TaskDialog.Show("Суперфильтр.", ex.Message);
#endif
            } finally {
                AppDomain.CurrentDomain.AssemblyResolve -= AppDomainExtensions.CurrentDomain_AssemblyResolve;
            }

            return Result.Succeeded;
        }
    }
}
