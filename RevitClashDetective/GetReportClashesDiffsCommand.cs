using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;

using RevitClashDetective.Models;
using RevitClashDetective.Models.RevitClashReport;
using RevitClashDetective.ViewModels.Navigator;
using RevitClashDetective.Views;

namespace RevitClashDetective {
    //TODO эта команда для дебага и отладки логики выполнения плагина
    [Transaction(TransactionMode.Manual)]
    public class GetReportClashesDiffsCommand : BasePluginCommand {
        public GetReportClashesDiffsCommand() {
            PluginName = "Различия в отчетах о коллизиях";
        }

        protected override void Execute(UIApplication uiApplication) {

            var revitRepository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);

            var pluginClashPath = @"";
            var pluginClashes = ReportLoader.GetClashes(revitRepository, pluginClashPath);

            var revitFilePath = @"";
            var revitClashes = ReportLoader.GetClashes(revitRepository, revitFilePath);

            var navisFilePath = @"";
            var navisClashes = ReportLoader.GetClashes(revitRepository, navisFilePath);

            var mainViewModlel = new ClashReportDiffViewModel(revitRepository, revitClashes, pluginClashes);

            var window = new ClashReportDiffView() { DataContext = mainViewModlel };
            window.Show();
        }
    }
}
