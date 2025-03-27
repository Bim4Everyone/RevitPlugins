using System.Globalization;
using System.Reflection;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WPF.Views;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;
using dosymep.Xpf.Core.Ninject;

using Ninject;

using RevitPlatformSettings.Extensions;
using RevitPlatformSettings.Factories;
using RevitPlatformSettings.Model;
using RevitPlatformSettings.Services;
using RevitPlatformSettings.ViewModels;
using RevitPlatformSettings.ViewModels.Settings;
using RevitPlatformSettings.Views;
using RevitPlatformSettings.Views.Pages;

using Wpf.Ui.Abstractions;

using Application = Autodesk.Revit.ApplicationServices.Application;

namespace RevitPlatformSettings {
    [Transaction(TransactionMode.Manual)]
    public class PlatformSettingsCommand : BasePluginCommand {
        public PlatformSettingsCommand() {
            PluginName = "Настройки платформы";
        }

        protected override void Execute(UIApplication uiApplication) {
            Notification(OpenSettingsWindow(uiApplication));
        }

        public bool? OpenSettingsWindow(UIApplication uiApplication) {
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.BindPages();
                kernel.BindViewModels();
                kernel.BindExtensions();
                
                kernel.Bind<INavigationViewPageProvider>()
                    .To<NavigationViewPageProvider>()
                    .InSingletonScope();

                // Используем сервис обновления тем для WinUI
                kernel.UseWpfUIThemeUpdater();

                kernel.Bind<OpenSourceWindow>().ToSelf();
                kernel.BindMainWindow<MainViewModel, MainWindow>();

                string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

                kernel.UseWpfLocalization(
                    $"/{assemblyName};component/assets/localization/language.xaml",
                    CultureInfo.GetCultureInfo("ru-RU"));

                return kernel.Get<MainWindow>().ShowDialog();
            }
        }
    }
}
