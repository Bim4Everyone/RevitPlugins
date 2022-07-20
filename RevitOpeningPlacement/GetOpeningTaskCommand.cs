using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.ViewModels.Navigator;
using RevitOpeningPlacement.Views;

namespace RevitOpeningPlacement {
    [Transaction(TransactionMode.Manual)]
    public class GetOpeningTaskCommand : BasePluginCommand {
        public GetOpeningTaskCommand() {
            PluginName = "Навигатор по заданиям";
        }

        protected override void Execute(UIApplication uiApplication) {
            RevitClashDetective.Models.RevitRepository clashRevitRepository
                = new RevitClashDetective.Models.RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);
            RevitRepository revitRepository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);

            var viewModel = new OpeningsViewModel(revitRepository, clashRevitRepository);

            var window = new NavigatorView() { Title = PluginName, DataContext = viewModel };
            var helper = new WindowInteropHelper(window) { Owner = uiApplication.MainWindowHandle };

            window.Show();
        }
    }
}