using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit.ServerClient;
using dosymep.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;
using dosymep.Xpf.Core.Ninject;

using Ninject;

using RevitServerFolders.Models;
using RevitServerFolders.Services;
using RevitServerFolders.ViewModels;
using RevitServerFolders.Views;

using Application = Autodesk.Revit.ApplicationServices.Application;

namespace RevitServerFolders;
[Transaction(TransactionMode.Manual)]
internal sealed class FileServerExportCommand : BasePluginCommand {
    public FileServerExportCommand() {
        PluginName = "Экспорт RVT файлов из RS";
    }

    protected override void Execute(UIApplication uiApplication) {
        using var kernel = uiApplication.CreatePlatformServices();
        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();

        kernel.UseWpfUIProgressDialog<RsViewModel>();
        kernel.UseWpfUIProgressDialog<FileSystemViewModel>();

        kernel.Bind<RsModelObjectConfig>()
            .ToMethod(c => RsModelObjectConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

        kernel.Bind<IModelObjectService>()
            .To<RsModelObjectService>();
        kernel.Bind<IModelsExportService>()
            .To<RvtExportService>()
            .InSingletonScope();
        kernel.Bind<ILoggerService>()
            .ToMethod(c => PluginLoggerService)
            .InSingletonScope();

        kernel.UseXtraOpenFolderDialog<MainWindow>(
            initialDirectory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

        kernel.Bind<IReadOnlyCollection<IServerClient>>()
            .ToMethod(c => c.Kernel.Get<Application>()
                .GetRevitServerNetworkHosts()
                .Select(item => new ServerClientBuilder()
                    .SetServerName(item)
                    .SetServerVersion(ModuleEnvironment.RevitVersion)
                    .Build())
                .ToArray());

        kernel.Bind<ViewModels.Rs.MainViewModel>().ToSelf();
        kernel.BindMainWindow<RsViewModel, MainWindow>();

        kernel.UseWpfUIThemeUpdater();

        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        kernel.UseWpfLocalization($"/{assemblyName};component/assets/Localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        Notification(kernel.Get<MainWindow>());
    }
}
