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

using RevitRoughFinishingDesign.Models;
using RevitRoughFinishingDesign.Services;
using RevitRoughFinishingDesign.ViewModels;
using RevitRoughFinishingDesign.Views;
namespace RevitRoughFinishingDesign;
[Transaction(TransactionMode.Manual)]
public class RevitRoughFinishingDesignCommand : BasePluginCommand {
    public RevitRoughFinishingDesignCommand() {
        PluginName = "RevitRoughFinishingDesign";
    }
    protected override void Execute(UIApplication uiApplication) {
        using var kernel = uiApplication.CreatePlatformServices();
        _ = kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();
        _ = kernel.Bind<PluginConfig>()
            .ToMethod(c => PluginConfig.GetPluginConfig());
        _ = kernel.Bind<ICurveLoopsSimplifier>()
            .To<CurveLoopsSimplifier>()
            .InSingletonScope();
        _ = kernel.Bind<CreatesLinesForFinishing>()
            .To<CreatesLinesForFinishing>()
            .InSingletonScope();
        _ = kernel.Bind<MainViewModel>().ToSelf();
        _ = kernel.Bind<MainWindow>().ToSelf()
            .WithPropertyValue(nameof(Window.DataContext),
                c => c.Kernel.Get<MainViewModel>())
            .WithPropertyValue(nameof(PlatformWindow.LocalizationService),
                c => c.Kernel.Get<ILocalizationService>());
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        _ = kernel.UseXtraLocalization(
            $"/{assemblyName};component/Localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));
        Notification(kernel.Get<MainWindow>());
    }
}
