using System.Globalization;
using System.Reflection;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitSplitMepCurve.Models;
using RevitSplitMepCurve.Services.Core;
using RevitSplitMepCurve.Services.Providers;
using RevitSplitMepCurve.ViewModels;
using RevitSplitMepCurve.Views;

namespace RevitSplitMepCurve;

[Transaction(TransactionMode.Manual)]
public class RevitSplitMepCurveCommand : BasePluginCommand {
    public RevitSplitMepCurveCommand() {
        PluginName = "Разделить по уровню СМР";
    }

    protected override void Execute(UIApplication uiApplication) {
        using var kernel = uiApplication.CreatePlatformServices();

        kernel.Bind<RevitRepository>().ToSelf().InSingletonScope();

        kernel.Bind<PluginConfig>()
            .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

        BindCoreServices(kernel);
        BindProviders(kernel);
        BindWindows(kernel);

        kernel.UseWpfUIThemeUpdater();
        kernel.UseWpfUIMessageBox<MainViewModel>();

        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        var localization = kernel.Get<ILocalizationService>();
        var messageBox = kernel.Get<IMessageBoxService>();
        var repository = kernel.Get<RevitRepository>();

        if(repository.GetDuplicateElevationLevels().Count > 0) {
            messageBox.Show(
                localization.GetLocalizedString("Error.DuplicateLevels"),
                localization.GetLocalizedString("MainWindow.Title"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        Notification(kernel.Get<MainWindow>());
    }

    private void BindCoreServices(IKernel kernel) {
        kernel.Bind<IErrorsService>().To<ErrorsService>().InSingletonScope();
        kernel.Bind<ErrorsWindowService>().ToSelf().InSingletonScope();
    }

    private void BindProviders(IKernel kernel) {
        kernel.Bind<PipesProvider>().ToSelf().InSingletonScope();
        kernel.Bind<DuctsProvider>().ToSelf().InSingletonScope();
        kernel.Bind<IElementsProvider>().To<PipesProvider>().InSingletonScope();
        kernel.Bind<IElementsProvider>().To<DuctsProvider>().InSingletonScope();
    }

    private void BindWindows(IKernel kernel) {
        kernel.BindMainWindow<MainViewModel, MainWindow>();
        kernel.BindOtherWindow<ViewModels.Errors.ErrorsViewModel, ErrorsWindow>();
    }
}
