using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;

namespace RevitFamilyExplorer {
    [Transaction(TransactionMode.Manual)]
    public class FamilyExplorerCommand : IExternalCommand {
        public static readonly Guid DockPanelId = new Guid("5469DADF-AF78-496F-BB93-B0E7B4D61EF6");

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
            var dockPanelId = new DockablePaneId(DockPanelId);
            DockablePane panel = uiApplication.GetDockablePane(dockPanelId);
            if(panel.IsShown()) {
                panel.Hide();
            } else {
                panel.Show();
            }
        }
    }
}