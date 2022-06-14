using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;

using RevitClashDetective.Models;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.ViewModels;
using RevitClashDetective.Views;

namespace RevitClashDetective {

    [Transaction(TransactionMode.Manual)]
    public class DetectiveClashesCommand : BasePluginCommand {
        public DetectiveClashesCommand() {
            PluginName = "Поиск коллизий";
        }

        protected override void Execute(UIApplication uiApplication) {
            var revitRepository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);
            var filterConfig = FiltersConfig.GetFiltersConfig();
            var checkConfig = ChecksConfig.GetFiltersConfig();
            var mainViewModlel = new MainViewModel(checkConfig, filterConfig, revitRepository);
            var window = new MainWindow() { DataContext = mainViewModlel };
            window.ShowDialog();
        }
    }
}
