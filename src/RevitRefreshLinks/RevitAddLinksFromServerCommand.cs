using System.Globalization;
using System.Reflection;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WPF.Views;
using dosymep.Xpf.Core.Ninject;

using Ninject;

using RevitRefreshLinks.Models;
using RevitRefreshLinks.ViewModels;
using RevitRefreshLinks.Views;

namespace RevitRefreshLinks {
    [Transaction(TransactionMode.Manual)]
    public class RevitAddLinksFromServerCommand : BasePluginCommand {
        public RevitAddLinksFromServerCommand() {
            PluginName = "Добавить связанные файлы из RS";
        }

        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();

                kernel.Bind<AddLinksFromFolderConfig>()
                    .ToMethod(c => AddLinksFromFolderConfig.GetPluginConfig());

                kernel.Bind<UpdateLinksViewModel>().ToSelf();
                kernel.Bind<UpdateLinksWindow>().ToSelf()
                    .WithPropertyValue(nameof(Window.DataContext),
                        c => c.Kernel.Get<UpdateLinksViewModel>())
                    .WithPropertyValue(nameof(PlatformWindow.LocalizationService),
                        c => c.Kernel.Get<ILocalizationService>());

                string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

                kernel.UseXtraLocalization(
                    $"/{assemblyName};component/Localization/Language.xaml",
                    CultureInfo.GetCultureInfo("ru-RU"));

                Notification(kernel.Get<UpdateLinksWindow>());
            }
        }
    }
}
