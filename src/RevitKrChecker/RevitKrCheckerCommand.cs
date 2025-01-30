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

using RevitKrChecker.Models;
using RevitKrChecker.ViewModels;
using RevitKrChecker.Views;
using RevitKrChecker.Views.Converters;

namespace RevitKrChecker {
    [Transaction(TransactionMode.Manual)]
    public class RevitKrCheckerCommand : BasePluginCommand {
        public RevitKrCheckerCommand() {
            PluginName = "Проверить модель";
        }

        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();

                kernel.Bind<PluginConfig>()
                    .ToMethod(c => PluginConfig.GetPluginConfig());

                kernel.Bind<MainVM>().ToSelf();
                kernel.Bind<MainWindow>().ToSelf()
                    .WithPropertyValue(nameof(Window.DataContext),
                        c => c.Kernel.Get<MainVM>())
                    .WithPropertyValue(nameof(PlatformWindow.LocalizationService),
                        c => c.Kernel.Get<ILocalizationService>());

                kernel.Bind<ReportVM>().ToSelf();
                kernel.Bind<ReportGroupingVM>().ToSelf();
                kernel.Bind<ReportWindow>().ToSelf()
                    .WithPropertyValue(nameof(PlatformWindow.LocalizationService),
                        c => c.Kernel.Get<ILocalizationService>());

                kernel.Bind<LocalizationConverter>().ToSelf()
                    .WithPropertyValue(nameof(LocalizationConverter.LocalizationService),
                        c => c.Kernel.Get<ILocalizationService>());

                string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

                kernel.UseXtraLocalization(
                    $"/{assemblyName};component/Localization/Language.xaml",
                    CultureInfo.GetCultureInfo("ru-RU"));

                Notification(kernel.Get<MainWindow>());
            }
        }
    }
}
