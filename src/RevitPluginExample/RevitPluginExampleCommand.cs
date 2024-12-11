using System.Globalization;
using System.Reflection;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.Xpf.Core.Ninject;

using Ninject;

using RevitPluginExample.Models;
using RevitPluginExample.Services;
using RevitPluginExample.ViewModels;
using RevitPluginExample.Views;

using Application = Autodesk.Revit.ApplicationServices.Application;

namespace RevitPluginExample {
    [Transaction(TransactionMode.Manual)]
    public class RevitPluginExampleCommand : BasePluginCommand {
        public RevitPluginExampleCommand() {
            PluginName = "RevitPluginExample";
        }

        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<UIApplication>()
                    .ToConstant(uiApplication)
                    .InTransientScope();
                kernel.Bind<Application>()
                    .ToConstant(uiApplication.Application)
                    .InTransientScope();

                kernel.Bind<PluginConfig>()
                    .ToMethod(c => PluginConfig.GetPluginConfig());

                kernel.Bind<IUIThemeUpdaterService>()
                    .To<ThemeUpdaterService>()
                    .InSingletonScope();

                kernel.Bind<MainViewModel>().ToSelf()
                    .InSingletonScope();
                kernel.Bind<MainWindow>().ToSelf()
                    .WithPropertyValue(nameof(BaseWindow.UIThemeService),
                         c => c.Kernel.Get<ThemeService>())
                    .WithPropertyValue(nameof(BaseWindow.UIThemeUpdaterService),
                        c => c.Kernel.Get<ThemeUpdaterService>())
                    .WithPropertyValue(nameof(Window.DataContext),
                        c => c.Kernel.Get<MainViewModel>())
                    .WithPropertyValue(nameof(BaseWindow.LocalizationService),
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
