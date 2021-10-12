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
    public class FamilyExplorerCommand : IExternalCommand {
        private readonly Guid _dockPanelId = new Guid("5469DADF-AF78-496F-BB93-B0E7B4D61EF6");

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

        private void Execute(UIApplication uiApplication) {
            DockablePane panel = null;
            var dockPanelId = new DockablePaneId(_dockPanelId);
            try {
                panel = uiApplication.GetDockablePane(dockPanelId);
            } catch(Autodesk.Revit.Exceptions.ArgumentException) {
                try {
                    uiApplication.RegisterDockablePane(dockPanelId, "Обозреватель семейств", new FamilyExplorerPanelProvider());
                } catch(Autodesk.Revit.Exceptions.ArgumentException) {
                }
                panel = uiApplication.GetDockablePane(dockPanelId);
            }

            if(panel.IsShown()) {
                panel.Hide();
            } else {
                panel.Show();
            }
        }
    }

    internal class FamilyExplorerPanelProvider : IDockablePaneProvider {
        public void SetupDockablePane(DockablePaneProviderData data) {
            var dataContext = new FamilyExplorerViewModel(new Models.FamilyRepository(@"D:\Temp\Familys"));
            var panel = new FamilyExplorerPanel() { DataContext = dataContext };

            data.FrameworkElement = panel;
            data.VisibleByDefault = true;
        }
    }
}