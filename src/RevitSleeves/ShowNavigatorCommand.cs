using System.Globalization;
using System.Reflection;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.Models.GraphicView;
using RevitClashDetective.Models.Handlers;
using RevitClashDetective.Models.Interfaces;

using RevitSleeves.Models;
using RevitSleeves.Models.Config;
using RevitSleeves.Services.Core;
using RevitSleeves.Services.Navigator;
using RevitSleeves.ViewModels.Filtration;
using RevitSleeves.ViewModels.Navigator;
using RevitSleeves.Views.Filtration;
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
        kernel.Bind<RevitEventHandler>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<ParameterFilterProvider>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<SleevePlacementSettingsConfig>()
            .ToMethod(c => SleevePlacementSettingsConfig.GetPluginConfig(
                new RevitClashConfigSerializer(
                    new RevitClashesSerializationBinder(), uiApplication.ActiveUIDocument.Document)));

        kernel.Bind<ISleeveStatusFinder>()
            .To<SleeveStatusFinder>()
            .InSingletonScope();
        kernel.Bind<IOpeningGeometryProvider>()
            .To<OpeningGeometryProvider>()
            .InSingletonScope();
        kernel.Bind<IStructureLinksProvider>()
            .To<UserSelectedStructureLinks>()
            .InSingletonScope();
        kernel.Bind<IGeometryUtils>()
            .To<GeometryUtils>()
            .InSingletonScope();
        kernel.Bind<IView3DProvider>()
            .To<SleeveView3dProvider>()
            .InSingletonScope();

        kernel.BindMainWindow<NavigatorViewModel, NavigatorWindow>();
        kernel.BindOtherWindow<StructureLinksSelectorViewModel, StructureLinksSelectorWindow>();
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
