using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;

using RevitFamilyExplorer.ViewModels;
using RevitFamilyExplorer.Views;

namespace RevitFamilyExplorer {
    [Transaction(TransactionMode.Manual)]
    public class RegisterFamilyExplorerCommand : IExternalCommand {
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
            var dockPanelId = new DockablePaneId(FamilyExplorerCommand.DockPanelId);
            if(!DockablePane.PaneIsRegistered(dockPanelId)) {
                uiApplication.RegisterDockablePane(dockPanelId, "Обозреватель семейств", new FamilyExplorerPanelProvider(uiApplication));
            }
        }
    }

    internal class FamilyExplorerPanelProvider : IDockablePaneProvider {
        private readonly UIApplication _uiApplication;

        public FamilyExplorerPanelProvider(UIApplication uiApplication) {
            _uiApplication = uiApplication;
        }

        public void SetupDockablePane(DockablePaneProviderData data) {
            var dataContext = new FamilyExplorerViewModel(new Models.RevitRepository(_uiApplication), new Models.FamilyRepository(@"D:\Temp\Familys"));
            var panel = new FamilyExplorerPanel() { DataContext = dataContext };

            data.FrameworkElement = panel;
            data.VisibleByDefault = true;
            data.InitialState.DockPosition = DockPosition.Bottom;
        }
    }
}