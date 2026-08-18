using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using Bim4Everyone.RevitFiltration.Controls;
using Bim4Everyone.RevitFiltration.Ninject;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.Xpf.Core.Ninject;

using Ninject;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Filtration;
using RevitClashDetective.Models.GraphicView;
using RevitClashDetective.Models.Handlers;
using RevitClashDetective.ViewModels.Navigator;
using RevitClashDetective.Views;
using RevitClashDetective.Views.Navigator;

using Wpf.Ui;

namespace RevitClashDetective;
[Transaction(TransactionMode.Manual)]
public class GetClashesCommand : BasePluginCommand {
    public GetClashesCommand() {
        PluginName = "Навигатор";
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

        kernel.UseLogicalFilterFactory();
        kernel.UseLogicalFilterProviderFactory();
        kernel.UseFilterContextParser();
        kernel.Bind<DataProvider>()
            .ToMethod(c => new FilterDataProvider(c.Kernel.Get<RevitRepository>()).CreateDataProvider())
            .InSingletonScope();
        kernel.Bind<LegacyFilterConverter>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<FiltersConfig>()
            .ToMethod(c => {
                var repo = c.Kernel.Get<RevitRepository>();
                string path = Path.Combine(repo.GetObjectName(), repo.GetDocumentName());
                return FiltersConfig.GetFiltersConfig(path, repo.Doc);
            });
        kernel.Bind<FilterContextsProvider>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<NavigatorViewModel>()
            .ToSelf()
            .InSingletonScope()
            .WithConstructorArgument("reportName", (string) null);
        kernel.Bind<IContentDialogService>()
            .To<ContentDialogService>()
            .InSingletonScope();
        kernel.Bind<SettingsConfig>()
            .ToMethod(c => SettingsConfig.GetSettingsConfig(c.Kernel.Get<IConfigSerializer>()));
        kernel.UseWpfOpenFileDialog<NavigatorViewModel>(
            filter: "NavisClashReport (*.xml)|*.xml|PluginClashReport (*.json)|*.json");
        kernel.UseWpfSaveFileDialog<NavigatorViewModel>();
        kernel.UseXtraMessageBox<NavigatorViewModel>();
        kernel.Bind<NavigatorWindow>()
            .ToSelf()
            .InSingletonScope()
            .WithPropertyValue(nameof(Window.DataContext), c => c.Kernel.Get<NavigatorViewModel>());

        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/Localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        kernel.Get<NavigatorWindow>().Show();
    }
}
