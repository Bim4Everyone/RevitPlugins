using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Superfilter {
    public class SuperfilterCommand : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            AppDomain.CurrentDomain.AssemblyResolve += AppDomainExtensions.CurrentDomain_AssemblyResolve;
            try {
                var viewModel = new RevitViewModel(commandData.Application.Application, commandData.Application.ActiveUIDocument.Document);

                var window = new MainWindow() { DataContext = viewModel };
                new WindowInteropHelper(window) { Owner = commandData.Application.MainWindowHandle };

                if(window.ShowDialog() == true) {

                }
            } catch(Exception ex) {
#if DEBUG
                System.Windows.MessageBox.Show(ex.ToString(), "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
#else
                System.Windows.MessageBox.Show(ex.Message, "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
#endif
            } finally {
                AppDomain.CurrentDomain.AssemblyResolve -= AppDomainExtensions.CurrentDomain_AssemblyResolve;
            }

            return Result.Succeeded;
        }
    }
}
