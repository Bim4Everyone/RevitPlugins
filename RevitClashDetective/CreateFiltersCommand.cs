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
using RevitClashDetective.ViewModels.FilterCreatorViewModels;
using RevitClashDetective.Views;

namespace RevitClashDetective {

    [Transaction(TransactionMode.Manual)]
    public class CreateFiltersCommand : BasePluginCommand {
        protected override void Execute(UIApplication uiApplication) {
            var revitRepository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);
            var viewModlel = new FiltersViewModel(revitRepository, FiltersConfig.GetFiltersConfig());
            var window = new FilterCreatorView() { DataContext = viewModlel };
            var helper = new WindowInteropHelper(window) { Owner = uiApplication.MainWindowHandle };
            window.ShowDialog();
        }
    }
}
