
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.RevitClashReport;
using RevitClashDetective.ViewModels.Navigator;
using RevitClashDetective.Views;

namespace RevitClashDetective {
    [Transaction(TransactionMode.Manual)]
    public class GetReportClashesDiffsCommand : BasePluginCommand {
        public GetReportClashesDiffsCommand() {
            PluginName = "Различия в отчетах о коллизиях";
        }

        protected override void Execute(UIApplication uiApplication) {

            var revitRepository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);

            var checkName = @"";
            var pluginClashes = ClashesConfig.GetClashesConfig(revitRepository.GetObjectName(), checkName).Clashes;

            var revitFilePath = @"";
            var revitClashes = new ReportLoader(revitRepository, revitFilePath).GetClashes();

            var mainViewModlel = new ClashReportDiffViewModel(revitRepository, revitClashes, pluginClashes);
           
            var window = new ClashReportDiffView() { DataContext = mainViewModlel };
            window.Show();
        }
    }
}