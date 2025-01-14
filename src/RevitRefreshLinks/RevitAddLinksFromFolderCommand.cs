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

namespace RevitRefreshLinks {
    [Transaction(TransactionMode.Manual)]
    public class RevitAddLinksFromFolderCommand : BasePluginCommand {
        public RevitAddLinksFromFolderCommand() {
            PluginName = "Добавить связанные файлы из папки";
        }

        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<UIApplication>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<ILinksProvider>()
                    .To<LinksProvider>()
                    .InSingletonScope();
                kernel.Bind<ILinksLoader>()
                    .To<LinksLoader>()
                    .InSingletonScope();

                kernel.Bind<AddLinksFromFolderConfig>()
                    .ToMethod(c => AddLinksFromFolderConfig.GetPluginConfig())
                    .InTransientScope();

                string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                kernel.UseXtraLocalization(
                    $"/{assemblyName};component/Localization/Language.xaml",
                    CultureInfo.GetCultureInfo("ru-RU"));
                var localizationService = kernel.Get<ILocalizationService>();
                kernel.UseXtraOpenFileDialog<LinksProvider>(
                    title: localizationService.GetLocalizedString("SelectLinksFromFolderDialog.Title"),
                    filter: localizationService.GetLocalizedString("SelectLinksFromFolderDialog.Filter"),
                    initialDirectory: GetInitialFolder(kernel),
                    multiSelect: true);

                var links = kernel.Get<ILinksProvider>().GetFolderLinks();
                kernel.Get<ILinksLoader>().LoadLinks(links);
                Notification(true);
            }
        }

        private string GetInitialFolder(IKernel kernel) {
            return kernel.Get<AddLinksFromFolderConfig>()
                .GetSettings(kernel.Get<UIApplication>().ActiveUIDocument.Document)
                .InitialFolderPath ?? string.Empty;
        }
    }
}
