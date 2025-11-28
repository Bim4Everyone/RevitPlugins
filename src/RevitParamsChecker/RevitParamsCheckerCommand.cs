using System.Globalization;
using System.Reflection;
using System.Windows.Controls;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitParamsChecker.Models;
using RevitParamsChecker.Models.Rules;
using RevitParamsChecker.Services;
using RevitParamsChecker.ViewModels;
using RevitParamsChecker.ViewModels.Checks;
using RevitParamsChecker.ViewModels.Dashboard;
using RevitParamsChecker.ViewModels.Filtration;
using RevitParamsChecker.ViewModels.Results;
using RevitParamsChecker.ViewModels.Rules;
using RevitParamsChecker.Views;
using RevitParamsChecker.Views.Checks;
using RevitParamsChecker.Views.Dashboard;
using RevitParamsChecker.Views.Filtration;
using RevitParamsChecker.Views.Results;
using RevitParamsChecker.Views.Rules;

using Wpf.Ui.Abstractions;
using Wpf.Ui.Controls;

namespace RevitParamsChecker;

[Transaction(TransactionMode.Manual)]
public class RevitParamsCheckerCommand : BasePluginCommand {
    public RevitParamsCheckerCommand() {
        PluginName = "Проверка параметров";
    }

    protected override void Execute(UIApplication uiApplication) {
        using IKernel kernel = uiApplication.CreatePlatformServices();

        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();

        BindViewModels(kernel);
        BindPages(kernel);
        kernel.Bind<INavigationViewPageProvider>()
            .To<NavigationViewPageProvider>()
            .InSingletonScope();

        kernel.UseWpfUIThemeUpdater();

        kernel.BindMainWindow<MainViewModel, MainWindow>();

        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        Notification(kernel.Get<MainWindow>());
    }

    private void BindPages(IKernel kernel) {
        kernel.Bind<DashboardPage>()
            .ToSelf()
            .InSingletonScope()
            .WithPropertyValue(nameof(Page.DataContext), c => c.Kernel.Get<DashboardPageViewModel>());
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

    private void BindViewModels(IKernel kernel) {
        kernel.Bind<DashboardPageViewModel>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<ChecksPageViewModel>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<RulesPageViewModel>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<FiltrationPageViewModel>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<ResultsPageViewModel>()
            .ToSelf()
            .InSingletonScope();
    }
}
