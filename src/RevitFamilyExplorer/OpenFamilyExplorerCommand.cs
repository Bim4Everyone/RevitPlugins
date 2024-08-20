using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;
using dosymep.Bim4Everyone;

using RevitFamilyExplorer.ViewModels;
using RevitFamilyExplorer.Views;

namespace RevitFamilyExplorer {
    [Transaction(TransactionMode.Manual)]
    public class OpenFamilyExplorerCommand : BasePluginCommand {
        public OpenFamilyExplorerCommand() {
            PluginName = "Обозреватель семейств";
        }
        
        protected override void Execute(UIApplication uiApplication) {
            var configPath = @"W:\Проектный институт\Отд.стандарт.BIM и RD\BIM-Ресурсы\5-Надстройки\99.FamilyExplorer";
            configPath = Path.Combine(configPath, uiApplication.Application.VersionNumber);
            var dataContext = new FamilyExplorerViewModel(new Models.RevitRepository(uiApplication), new Models.FamilyRepository(configPath));
            var panel = new FamilyExplorerPanel() { DataContext = dataContext };

            var window = new System.Windows.Window { Content = panel, WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner };
            var helper = new WindowInteropHelper(window) { Owner = uiApplication.MainWindowHandle };

            window.ShowDialog();
        }
    }
}
