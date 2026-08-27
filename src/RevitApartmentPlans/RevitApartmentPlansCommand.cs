using System;
using System.Globalization;
using System.Reflection;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitApartmentPlans.Models;
using RevitApartmentPlans.Services;
using RevitApartmentPlans.ViewModels;
using RevitApartmentPlans.Views;

namespace RevitApartmentPlans;
[Transaction(TransactionMode.Manual)]
public class RevitApartmentPlansCommand : BasePluginCommand {
    public RevitApartmentPlansCommand() {
        PluginName = "Планы квартир";
    }

    protected override void Execute(UIApplication uiApplication) {
        using var kernel = uiApplication.CreatePlatformServices();
        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<IBoundsCalculationService>()
            .To<BoundsCalculationService>()
            .InSingletonScope();
        kernel.Bind<ICurveLoopsMerger>()
            .To<CurveLoopsMerger>()
            .InSingletonScope();
        kernel.Bind<ICurveLoopsOffsetter>()
            .To<CurveLoopsOffsetter>()
            .InSingletonScope();
        kernel.Bind<ICurveLoopsSimplifier>()
            .To<CurveLoopsSimplifier>()
            .InSingletonScope();
        kernel.Bind<IRectangleLoopProvider>()
            .To<RectangleLoopProvider>()
            .InSingletonScope();
        kernel.Bind<IViewPlanCreationService>()
            .To<ViewPlanCreationService>()
            .InSingletonScope();
        kernel.Bind<ILengthConverter>()
            .To<Services.LengthConverter>()
            .InSingletonScope();
        kernel.Bind<PluginConfig>()
            .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));
        kernel.Bind<LinkFilterProvider>()
            .ToSelf()
            .InSingletonScope();

        kernel.UseWpfUIProgressDialog<MainViewModel>();
        kernel.UseWpfUIMessageBox<ApartmentsViewModel>();
        kernel.Bind<ApartmentsViewModel>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<ViewTemplatesViewModel>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<ViewTemplateAdditionViewModel>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<ViewTemplateAdditionWindow>()
            .ToSelf()
            .InTransientScope()
            .WithPropertyValue(nameof(Window.DataContext), c => c.Kernel.Get<ViewTemplateAdditionViewModel>())
            .WithPropertyValue(nameof(Window.Owner), c => c.Kernel.Get<MainWindow>());
        kernel.BindMainWindow<MainViewModel, MainWindow>();

        kernel.UseWpfUIThemeUpdater();

        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        kernel.UseWpfLocalization($"/{assemblyName};component/assets/Localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        var repo = kernel.Get<RevitRepository>();
        var msg = kernel.Get<IMessageBoxService>();
        var localization = kernel.Get<ILocalizationService>();
        CheckViews(repo, msg, localization);
        Notification(kernel.Get<MainWindow>());
    }


    private void CheckViews(
        RevitRepository revitRepository,
        IMessageBoxService messageBoxService,
        ILocalizationService localization) {
        if(!revitRepository.ActiveViewIsPlan()) {
            var result = messageBoxService.Show(
                localization.GetLocalizedString("Errors.ActiveViewMustBePlan"),
                PluginName,
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
            throw new OperationCanceledException();
        }
    }
}
