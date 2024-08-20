using System;
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

using RevitExamplePlugin.Models;
using RevitExamplePlugin.ViewModels;
using RevitExamplePlugin.Views;

namespace RevitExamplePlugin {
    [Transaction(TransactionMode.Manual)]
    public class RevitExamplePluginCommand : BasePluginCommand {
        public RevitExamplePluginCommand() {
            PluginName = "C# Button";
        }

        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<WallRevitRepository>()
                    .ToSelf()
                    .InSingletonScope();

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
                
                CheckViews(kernel);
                Notification(kernel.Get<MainWindow>());
            }
        }

        private void CheckViews(IKernel kernel) {
            var wallRepository = kernel.Get<WallRevitRepository>();
            var localizationService = kernel.Get<ILocalizationService>();
            
            if(!wallRepository.ActiveViewIsPlan()) {
                var messageBoxService = kernel.Get<IMessageBoxService>();
                messageBoxService.Show(
                    localizationService.GetLocalizedString("TaskDialog.ActiveViewUserPromt"),
                    localizationService.GetLocalizedString("TaskDialog.ActiveViewTitle"),
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                throw new OperationCanceledException();
            }
        }
    }
}
