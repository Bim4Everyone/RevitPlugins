using System;
using System.Globalization;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;
using dosymep.Xpf.Core.Ninject;

using Ninject;

using RevitServerFolders.Models;
using RevitServerFolders.Services;
using RevitServerFolders.ViewModels;
using RevitServerFolders.Views;

namespace RevitServerFolders;
[Transaction(TransactionMode.Manual)]
internal sealed class NavisworksExportCommand : BasePluginCommand {
    public NavisworksExportCommand() {
        PluginName = "Экспорт вида Navisworks";
    }

    protected override void Execute(UIApplication uiApplication) {
        if(!OptionalFunctionalityUtils.IsNavisworksExporterAvailable()) {
            throw new InvalidOperationException(
                "Отсутствует плагин Navisworks NWC Export Utility. Необходимо установить его.");
        }
        using var kernel = uiApplication.CreatePlatformServices();
        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();

        kernel.UseWpfUIProgressDialog<FileSystemViewModel>();

        kernel.Bind<FileModelObjectConfig>()
            .ToMethod(c => FileModelObjectConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

        kernel.Bind<IModelObjectService>()
            .To<FileSystemModelObjectService>();
        kernel.Bind<IModelsExportService>()
            .To<NwcExportService>()
            .InSingletonScope();
        kernel.Bind<ILoggerService>()
            .ToMethod(c => PluginLoggerService)
            .InSingletonScope();

        kernel.UseXtraOpenFolderDialog<MainWindow>(
            initialDirectory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

        kernel.BindMainWindow<FileSystemViewModel, MainWindow>();

        kernel.UseWpfUIThemeUpdater();

        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        kernel.UseWpfLocalization($"/{assemblyName};component/assets/Localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        Notification(kernel.Get<MainWindow>());
    }
}
