using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;

using RevitClashDetective.Models;
using RevitClashDetective.ViewModels.Navigator;
using RevitClashDetective.Views;

namespace RevitClashDetective {
    //TODO эта команда для дебага и отладки логики выполнения плагина
    [Transaction(TransactionMode.Manual)]
    public class GetRevitReportClashesCommand : BasePluginCommand {
        public GetRevitReportClashesCommand() {
            PluginName = "Отчет Revit-а о пересечениях";
        }

        protected override void Execute(UIApplication uiApplication) {
            var revitRepository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);
            var mainViewModlel = new RevitReportClashesViewModel(revitRepository);
            var window = new RevitReportClashNavigator() { DataContext = mainViewModlel };
            window.Show();
        }
    }
}