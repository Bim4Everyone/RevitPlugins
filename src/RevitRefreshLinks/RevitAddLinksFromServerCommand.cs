using System.Globalization;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitRefreshLinks.Extensions;
using RevitRefreshLinks.Models;
using RevitRefreshLinks.Services;
using RevitRefreshLinks.ViewModels;

namespace RevitRefreshLinks;
[Transaction(TransactionMode.Manual)]
public class RevitAddLinksFromServerCommand : BasePluginCommand {
    public RevitAddLinksFromServerCommand() {
        PluginName = "Добавить связанные файлы из RS";
    }

    protected override void Execute(UIApplication uiApplication) {
        using var kernel = uiApplication.CreatePlatformServices();
        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<IServerSourceLinksProvider>()
            .To<RsLinksProvider>()
            .InSingletonScope();
        kernel.Bind<ILinksLoader>()
            .To<LinksLoader>()
            .InSingletonScope();
        kernel.Bind<IConfigProvider>()
            .To<ConfigProvider>()
            .InSingletonScope();

        kernel.Bind<AddLinksFromServerConfig>()
            .ToMethod(c => AddLinksFromServerConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()))
            .InTransientScope();

        kernel.Bind<AddServerLinksViewModel>()
            .ToSelf()
            .InSingletonScope();

        kernel.UseWpfUIProgressDialog<AddServerLinksViewModel>();

        kernel.UseWpfUIThemeUpdater();

        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        kernel.UseWpfLocalization($"/{assemblyName};component/Localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));
        var localizationService = kernel.Get<ILocalizationService>();
        kernel.UseRsOpenFileDialog(
            title: localizationService.GetLocalizedString("RsOpenFileWindow.Title"),
            initialDirectory: GetInitialServerFolder(kernel),
            multiSelect: true);

        kernel.Get<AddServerLinksViewModel>().AddLinksCommand.Execute(default);
    }

    private string GetInitialServerFolder(IKernel kernel) {
        return kernel.Get<AddLinksFromServerConfig>()
            .GetSettings(kernel.Get<UIApplication>().ActiveUIDocument.Document)
            ?.InitialServerPath ?? string.Empty;
    }
}
