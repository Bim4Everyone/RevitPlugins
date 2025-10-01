using System.Globalization;
using System.Linq;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WpfCore.Ninject;

using Ninject;

using RevitClashDetective.Models;
using RevitClashDetective.Models.GraphicView;
using RevitClashDetective.Models.Handlers;
using RevitClashDetective.Models.RevitClashReport;
using RevitClashDetective.ViewModels.Navigator;
using RevitClashDetective.Views;

namespace RevitClashDetective;
//TODO эта команда для дебага и отладки логики выполнения плагина
[Transaction(TransactionMode.Manual)]
public class GetReportClashesDiffsCommand : BasePluginCommand {
    public GetReportClashesDiffsCommand() {
        PluginName = "Различия в отчетах о коллизиях";
    }

    protected override void Execute(UIApplication uiApplication) {
        using var kernel = uiApplication.CreatePlatformServices();
        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<RevitEventHandler>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<ParameterFilterProvider>()
            .ToSelf()
            .InSingletonScope();

        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/Localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));


        var revitRepository = kernel.Get<RevitRepository>();

        string pluginClashPath = @"";
        var pluginClashes = ReportLoader.GetReports(revitRepository, pluginClashPath);

        string revitFilePath = @"";
        var revitClashes = ReportLoader.GetReports(revitRepository, revitFilePath);

        string navisFilePath = @"";
        var navisClashes = ReportLoader.GetReports(revitRepository, navisFilePath);

        var mainViewModlel = new ClashReportDiffViewModel(revitRepository, revitClashes.First().Clashes, pluginClashes.First().Clashes);

        var window = new ClashReportDiffView() { DataContext = mainViewModlel };
        window.Show();
    }
}
