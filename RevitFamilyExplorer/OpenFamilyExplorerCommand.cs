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

using RevitFamilyExplorer.ViewModels;
using RevitFamilyExplorer.Views;

namespace RevitFamilyExplorer {
    [Transaction(TransactionMode.Manual)]
    public class OpenFamilyExplorerCommand : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            AppDomain.CurrentDomain.AssemblyResolve += AppDomainExtensions.CurrentDomain_AssemblyResolve;
            try {
                Execute(commandData.Application);
            } catch(Exception ex) {
#if D2020 || D2021 || D2022
                TaskDialog.Show("Обозреватель семейств.", ex.ToString());
#endif
            } finally {
                AppDomain.CurrentDomain.AssemblyResolve -= AppDomainExtensions.CurrentDomain_AssemblyResolve;
            }

            return Result.Succeeded;
        }

        public void Execute(UIApplication uiApplication) {
            var dataContext = new FamilyExplorerViewModel(new Models.RevitRepository(uiApplication), new Models.FamilyRepository(@"D:\Temp\Familys"));
            var panel = new FamilyExplorerPanel() { DataContext = dataContext };

            var window = new System.Windows.Window { Content = panel, WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner };
            new WindowInteropHelper(window) { Owner = uiApplication.MainWindowHandle };

            window.ShowDialog();
        }
    }
}
