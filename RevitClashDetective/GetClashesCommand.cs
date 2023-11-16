using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;

using RevitClashDetective.Models;
using RevitClashDetective.ViewModels.Navigator;
using RevitClashDetective.Views;

namespace RevitClashDetective {
    [Transaction(TransactionMode.Manual)]
    public class GetClashesCommand : BasePluginCommand {
        public GetClashesCommand() {
            PluginName = "Поиск коллизий";
        }

        protected override void Execute(UIApplication uiApplication) {
            var revitRepository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);
            var mainViewModlel = new ReportsViewModel(revitRepository);
            var window = new NavigatorView() { DataContext = mainViewModlel };
            window.Show();
        }
    }
}
