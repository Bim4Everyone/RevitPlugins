using System.Globalization;
using System.Reflection;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitSleeves.Models;
using RevitSleeves.Models.Config;
using RevitSleeves.Services.Core;
using RevitSleeves.Services.Navigator;
using RevitSleeves.ViewModels.Navigator;
using RevitSleeves.Views.Navigator;

namespace RevitSleeves;
[Transaction(TransactionMode.Manual)]
internal class ShowNavigatorCommand : BasePluginCommand {
    public ShowNavigatorCommand() {
        PluginName = "Навигатор";
    }


    protected override void Execute(UIApplication uiApplication) {
        using var kernel = uiApplication.CreatePlatformServices();

        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<RevitClashDetective.Models.RevitRepository>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<SleevePlacementSettingsConfig>()
            .ToMethod(c => SleevePlacementSettingsConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

        kernel.Bind<ISleeveStatusFinder>()
            .To<SleeveStatusFinder>()
            .InSingletonScope();
        kernel.Bind<IOpeningGeometryProvider>()
            .To<OpeningGeometryProvider>()
            .InSingletonScope();
        kernel.Bind<IStructureLinksProvider>()
            .To<AllLoadedStructureLinksProvider>()
            .InSingletonScope();

        kernel.BindMainWindow<NavigatorViewModel, NavigatorWindow>();
        kernel.UseWpfUIMessageBox<NavigatorViewModel>();
        kernel.UseWpfUIProgressDialog<NavigatorViewModel>();

        kernel.UseWpfUIThemeUpdater();

        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        var window = kernel.Get<NavigatorWindow>();
        var helper = new WindowInteropHelper(window) { Owner = uiApplication.MainWindowHandle };
        kernel.Get<NavigatorWindow>().Show();
    }
}
