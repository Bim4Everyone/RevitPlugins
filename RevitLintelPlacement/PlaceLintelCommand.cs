using System;
using System.IO;
using System.Linq;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;
using dosymep.Bim4Everyone;

using RevitLintelPlacement.Models;
using RevitLintelPlacement.ViewModels;
using RevitLintelPlacement.Views;

namespace RevitLintelPlacement {

    [Transaction(TransactionMode.Manual)]
    public class PlaceLintelCommand  : BasePluginCommand {
        public PlaceLintelCommand() {
            PluginName = "Расстановщик перемычек";
        }
        
        protected override void Execute(UIApplication uiApplication) {
            var lintelsConfig = LintelsConfig.GetLintelsConfig();
            var revitRepository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document, lintelsConfig);
               
            var mainViewModel = new MainViewModel(revitRepository);
            var window = new MainWindow() { DataContext = mainViewModel };
            var helper = new WindowInteropHelper(window) { Owner = uiApplication.MainWindowHandle };
            window.ShowDialog();
        }
    }
}
