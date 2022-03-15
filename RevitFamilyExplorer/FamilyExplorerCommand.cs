using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;
using dosymep.Bim4Everyone;

namespace RevitFamilyExplorer {
    [Transaction(TransactionMode.Manual)]
    public class FamilyExplorerCommand : BasePluginCommand {
        public static readonly Guid DockPanelId = new Guid("5469DADF-AF78-496F-BB93-B0E7B4D61EF6");
        
        public FamilyExplorerCommand() {
            PluginName = "Обозреватель семейств";
        }

        protected override void Execute(UIApplication uiApplication) {
            var dockPanelId = new DockablePaneId(DockPanelId);
            DockablePane panel = uiApplication.GetDockablePane(dockPanelId);
            if(panel.IsShown()) {
                panel.Hide();
            } else {
                panel.Show();
            }
        }
        
        public void ChangeVisiblePanel(UIApplication uiApplication) {
            Execute(uiApplication);
        }
    }
}