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

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.ViewModels;
using RevitOpeningPlacement.ViewModels.OpeningConfig;
using RevitOpeningPlacement.Views;

namespace RevitOpeningPlacement {
    [Transaction(TransactionMode.Manual)]
    public class SetOpeningTaskCommand : BasePluginCommand {
        public SetOpeningTaskCommand() {
            PluginName = "Настройка заданий на отверстия";
        }

        protected override void Execute(UIApplication uiApplication) {
            var openingConfig = OpeningConfig.GetOpeningConfig();
            var viewModel = new MainViewModel(uiApplication, openingConfig);

            var window = new MainWindow() { Title = PluginName, DataContext = viewModel };
            var helper = new WindowInteropHelper(window) { Owner = uiApplication.MainWindowHandle };

            window.ShowDialog();
        }
    }
}