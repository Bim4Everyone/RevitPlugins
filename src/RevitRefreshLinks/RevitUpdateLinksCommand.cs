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

using RevitRefreshLinks.Models;
using RevitRefreshLinks.Services;
using RevitRefreshLinks.ViewModels;
using RevitRefreshLinks.Views;

namespace RevitRefreshLinks {
    [Transaction(TransactionMode.Manual)]
    public class RevitUpdateLinksCommand : BasePluginCommand {
        public RevitUpdateLinksCommand() {
            PluginName = "Обновить связанные файлы";
        }

        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<UIApplication>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<ITwoSourceLinksProvider>()
                    .To<ManyFoldersLinksProvider>()
                    .InSingletonScope();
                kernel.Bind<ILinksLoader>()
                    .To<LinksLoader>()
                    .InSingletonScope();
                kernel.Bind<IConfigProvider>()
                    .To<ConfigProvider>()
                    .InSingletonScope();

                kernel.Bind<UpdateLinksConfig>()
                    .ToMethod(c => UpdateLinksConfig.GetPluginConfig());

                kernel.Bind<UpdateLinksViewModel>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<UpdateLinksWindow>()
                    .ToSelf()
                    .WithPropertyValue(nameof(Window.DataContext),
                        c => c.Kernel.Get<UpdateLinksViewModel>())
                    .WithPropertyValue(nameof(PlatformWindow.LocalizationService),
                        c => c.Kernel.Get<ILocalizationService>());

                string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                kernel.UseXtraLocalization(
                    $"/{assemblyName};component/Localization/Language.xaml",
                    CultureInfo.GetCultureInfo("ru-RU"));
                var localizationService = kernel.Get<ILocalizationService>();
                kernel.UseXtraOpenFolderDialog<ManyFoldersLinksProvider>(
                    title: localizationService.GetLocalizedString("SelectLocalFoldersDialog.Title"),
                    initialDirectory: GetInitialFolder(kernel),
                    multiSelect: true);

                Notification(kernel.Get<UpdateLinksWindow>());
            }
        }

        private string GetInitialFolder(IKernel kernel) {
            return kernel.Get<UpdateLinksConfig>()
                .GetSettings(kernel.Get<UIApplication>().ActiveUIDocument.Document)
                ?.InitialFolderPath ?? string.Empty;
        }
    }
}
