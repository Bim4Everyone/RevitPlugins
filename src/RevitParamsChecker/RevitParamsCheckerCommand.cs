using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using Bim4Everyone.RevitFiltration.Controls;
using Bim4Everyone.RevitFiltration.Controls.Extensions;
#if REVIT_2022
// TODO
using Bim4Everyone.RevitFiltration.Extensions;
#endif

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitParamsChecker.Models.Checks;
using RevitParamsChecker.Models.Filtration;
using RevitParamsChecker.Models.Results;
using RevitParamsChecker.Models.Revit;
using RevitParamsChecker.Models.Rules;
using RevitParamsChecker.Services;
using RevitParamsChecker.ViewModels;
using RevitParamsChecker.ViewModels.Checks;
using RevitParamsChecker.ViewModels.Filtration;
using RevitParamsChecker.ViewModels.Results;
using RevitParamsChecker.ViewModels.Rules;
using RevitParamsChecker.ViewModels.Utils;
using RevitParamsChecker.Views;
using RevitParamsChecker.Views.Checks;
using RevitParamsChecker.Views.Dashboard;
using RevitParamsChecker.Views.Filtration;
using RevitParamsChecker.Views.Results;
using RevitParamsChecker.Views.Rules;
using RevitParamsChecker.Views.Utils;

using Wpf.Ui;
using Wpf.Ui.Abstractions;

namespace RevitParamsChecker;

[Transaction(TransactionMode.Manual)]
public class RevitParamsCheckerCommand : BasePluginCommand {
    public RevitParamsCheckerCommand() {
        PluginName = "Проверка параметров";
    }

    protected override void Execute(UIApplication uiApplication) {
        IKernel kernel = uiApplication.CreatePlatformServices();

        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();

        BindPageViewModels(kernel);
        BindPages(kernel);
        BindUtilsViews(kernel);
        BindRepos(kernel);
        BindServices(kernel);
        BindConverters(kernel);
#if REVIT_2022
// TODO
        kernel.UseDefaultFactory();
#endif
        kernel.UseDefaultProviderFactory();
        kernel.UseDefaultContextParser();
        kernel.Bind<IDataProvider>()
            .To<FilterDataProvider>()
            .InSingletonScope();
        kernel.Bind<INavigationViewPageProvider>()
            .To<NavigationViewPageProvider>()
            .InSingletonScope();
        kernel.Bind<IContentDialogService>()
            .To<ContentDialogService>()
            .InSingletonScope();

        kernel.UseWpfUIThemeUpdater();

        kernel.BindMainWindow<MainViewModel, MainWindow>();
        kernel.UseWpfOpenFileDialog<MainViewModel>(filter: "JSON files (*.json)|*.json");
        kernel.UseWpfSaveFileDialog<MainViewModel>(filter: "JSON files (*.json)|*.json");
        kernel.UseWpfUIMessageBox<MainViewModel>();
        kernel.UseWpfUIProgressDialog<MainViewModel>();

        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        // kernel.Get<MainWindow>().Show();
        kernel.Get<MainWindow>().ShowDialog();
    }

    private void BindPages(IKernel kernel) {
        kernel.Bind<DashboardPage>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<ChecksPage>()
            .ToSelf()
            .InSingletonScope()
            .WithPropertyValue(nameof(Page.DataContext), c => c.Kernel.Get<ChecksPageViewModel>());
        kernel.Bind<RulesPage>()
            .ToSelf()
            .InSingletonScope()
            .WithPropertyValue(nameof(Page.DataContext), c => c.Kernel.Get<RulesPageViewModel>());
        kernel.Bind<FiltrationPage>()
            .ToSelf()
            .InSingletonScope()
            .WithPropertyValue(nameof(Page.DataContext), c => c.Kernel.Get<FiltrationPageViewModel>());
        kernel.Bind<ResultsPage>()
            .ToSelf()
            .InSingletonScope()
            .WithPropertyValue(nameof(Page.DataContext), c => c.Kernel.Get<ResultsPageViewModel>());
    }

    private void BindPageViewModels(IKernel kernel) {
        // если создать ConstructorArgument и прокинуть в WithConstructorArgument, почему-то не работает
        kernel.Bind<ChecksPageViewModel>()
            .ToSelf()
            .InSingletonScope()
            .WithConstructorArgument("openFileDialogService", c => c.Kernel.Get<MainViewModel>().OpenFileDialogService)
            .WithConstructorArgument("saveFileDialogService", c => c.Kernel.Get<MainViewModel>().SaveFileDialogService)
            .WithConstructorArgument("messageBoxService", c => c.Kernel.Get<MainViewModel>().MessageBoxService)
            .WithConstructorArgument("progressDialogFactory", c => c.Kernel.Get<MainViewModel>().ProgressDialogFactory);
        kernel.Bind<RulesPageViewModel>()
            .ToSelf()
            .InSingletonScope()
            .WithConstructorArgument("openFileDialogService", c => c.Kernel.Get<MainViewModel>().OpenFileDialogService)
            .WithConstructorArgument("saveFileDialogService", c => c.Kernel.Get<MainViewModel>().SaveFileDialogService)
            .WithConstructorArgument("messageBoxService", c => c.Kernel.Get<MainViewModel>().MessageBoxService);
        kernel.Bind<FiltrationPageViewModel>()
            .ToSelf()
            .InSingletonScope()
            .WithConstructorArgument("openFileDialogService", c => c.Kernel.Get<MainViewModel>().OpenFileDialogService)
            .WithConstructorArgument("saveFileDialogService", c => c.Kernel.Get<MainViewModel>().SaveFileDialogService)
            .WithConstructorArgument("messageBoxService", c => c.Kernel.Get<MainViewModel>().MessageBoxService);
        kernel.Bind<ResultsPageViewModel>()
            .ToSelf()
            .InSingletonScope();
    }

    private void BindUtilsViews(IKernel kernel) {
        kernel.Bind<NameEditorViewModel>()
            .ToSelf()
            .InTransientScope();
        kernel.Bind<NameEditorWindow>()
            .ToSelf()
            .InTransientScope()
            .WithPropertyValue(nameof(Window.Owner), c => c.Kernel.Get<MainWindow>());

        kernel.Bind<SelectableNamesViewModel>()
            .ToSelf()
            .InTransientScope();
        kernel.Bind<SelectableNamesDialog>()
            .ToSelf()
            .InTransientScope();
    }

    private void BindRepos(IKernel kernel) {
        kernel.Bind<JsonSerializationService>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<FiltersConfig>()
            .ToMethod(c => FiltersConfig.GetConfig(c.Kernel.Get<JsonSerializationService>()));
        kernel.Bind<FiltersRepository>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<RulesConfig>()
            .ToMethod(c => RulesConfig.GetConfig(c.Kernel.Get<JsonSerializationService>()));
        kernel.Bind<RulesRepository>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<ChecksConfig>()
            .ToMethod(c => ChecksConfig.GetConfig(c.Kernel.Get<JsonSerializationService>()));
        kernel.Bind<ChecksRepository>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<CheckResultsRepository>()
            .ToSelf()
            .InSingletonScope();
    }

    private void BindConverters(IKernel kernel) {
        kernel.Bind<FiltersConverter>()
            .ToSelf()
            .InSingletonScope()
            .WithConstructorArgument("serializer", c => c.Kernel.Get<JsonSerializationService>());
        kernel.Bind<ChecksConverter>()
            .ToSelf()
            .InSingletonScope()
            .WithConstructorArgument("serializer", c => c.Kernel.Get<JsonSerializationService>());
        kernel.Bind<RulesConverter>()
            .ToSelf()
            .InSingletonScope()
            .WithConstructorArgument("serializer", c => c.Kernel.Get<JsonSerializationService>());
    }

    private void BindServices(IKernel kernel) {
        kernel.Bind<NamesService>()
            .ToSelf()
            .InSingletonScope();
    }
}
