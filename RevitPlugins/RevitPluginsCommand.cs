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

using RevitPlugins.ViewModels;
using RevitPlugins.Views;

namespace RevitPlugins {
    [Transaction(TransactionMode.Manual)]
    public class RevitPluginsCommand : BasePluginCommand {
        public RevitPluginsCommand() {
            PluginName = "RevitPlugins";
        }

        protected override void Execute(UIApplication uiApplication) {
            var viewModel = new MainViewModel(uiApplication);

            var window = new MainWindow() { Title = PluginName, DataContext = viewModel};
            var helper = new WindowInteropHelper(window) {Owner = uiApplication.MainWindowHandle};

            window.ShowDialog();
        }
    }
}