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
using dosymep.Bim4Everyone;

using RevitOpeningPlacement.ViewModels;
using RevitOpeningPlacement.ViewModels.OpeningConfig;
using RevitOpeningPlacement.Views;

namespace RevitOpeningPlacement {
    [Transaction(TransactionMode.Manual)]
    public class RevitOpeningPlacementCommand : BasePluginCommand {
        public RevitOpeningPlacementCommand() {
            PluginName = "RevitOpeningPlacement";
        }

        protected override void Execute(UIApplication uiApplication) {
            var viewModel = new MainViewModel(uiApplication);

            var window = new MainWindow() { Title = PluginName, DataContext = viewModel };
            var helper = new WindowInteropHelper(window) { Owner = uiApplication.MainWindowHandle };

            window.ShowDialog();
        }
    }
}