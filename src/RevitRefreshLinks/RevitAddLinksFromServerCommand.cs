using System.Globalization;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.Xpf.Core.Ninject;

using Ninject;

using RevitRefreshLinks.Models;
using RevitRefreshLinks.Services;
using RevitRefreshLinks.ViewModels;

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
                kernel.Bind<UIApplication>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<IOneSourceLinksProvider>()
                    .To<OneRsFolderLinksProvider>()
                    .InSingletonScope();
                kernel.Bind<ILinksLoader>()
                    .To<LinksLoader>()
                    .InSingletonScope();
                kernel.Bind<AddLinksViewModel>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<IConfigProvider>()
                    .To<ConfigProvider>()
                    .InSingletonScope();

                kernel.Bind<AddLinksFromServerConfig>()
                    .ToMethod(c => AddLinksFromServerConfig.GetPluginConfig())
                    .InTransientScope();

                string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                kernel.UseXtraLocalization(
                    $"/{assemblyName};component/Localization/Language.xaml",
                    CultureInfo.GetCultureInfo("ru-RU"));
                var localizationService = kernel.Get<ILocalizationService>();

                var vm = kernel.Get<AddLinksViewModel>();
                Notification(vm.ShowWindow());
            }
        }
    }
}
