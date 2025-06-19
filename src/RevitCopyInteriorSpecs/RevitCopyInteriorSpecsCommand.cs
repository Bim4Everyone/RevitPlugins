using System.Globalization;
using System.Reflection;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WPF.Views;
using dosymep.Xpf.Core.Ninject;

using Ninject;

using RevitCopyInteriorSpecs.Models;
using RevitCopyInteriorSpecs.Services;
using RevitCopyInteriorSpecs.ViewModels;
using RevitCopyInteriorSpecs.Views;

namespace RevitCopyInteriorSpecs;
[Transaction(TransactionMode.Manual)]
public class RevitCopyInteriorSpecsCommand : BasePluginCommand {
    public RevitCopyInteriorSpecsCommand() {
        PluginName = "Копирование спецификаций АИ";
    }

    protected override void Execute(UIApplication uiApplication) {
        using var kernel = uiApplication.CreatePlatformServices();
        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<SpecificationService>()
            .ToSelf();

        kernel.Bind<PluginConfig>()
            .ToMethod(c => PluginConfig.GetPluginConfig());

        kernel.Bind<MainViewModel>().ToSelf();
        kernel.Bind<MainWindow>().ToSelf()
            .WithPropertyValue(nameof(Window.DataContext),
                c => c.Kernel.Get<MainViewModel>())
            .WithPropertyValue(nameof(PlatformWindow.LocalizationService),
                c => c.Kernel.Get<ILocalizationService>());

        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        kernel.UseXtraLocalization(
            $"/{assemblyName};component/Localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        Notification(kernel.Get<MainWindow>());
    }
}
