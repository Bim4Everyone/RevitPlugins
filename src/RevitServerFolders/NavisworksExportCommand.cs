using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

using Autodesk.Revit.ApplicationServices;
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

using Wpf.Ui;

namespace RevitServerFolders;
[Transaction(TransactionMode.Manual)]
internal sealed class NavisworksExportCommand : BasePluginCommand {
    public NavisworksExportCommand() {
        PluginName = "Экспорт вида Navisworks";
    }

    protected override void Execute(UIApplication uiApplication) {
        using var kernel = uiApplication.CreatePlatformServices();
        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();

        kernel.UseWpfUIProgressDialog<FileSystemViewModel>();

        kernel.Bind<FileModelObjectConfig>()
            .ToMethod(c => FileModelObjectConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

        kernel.Bind<IModelObjectService>()
            .To<FileSystemModelObjectService>();
        kernel.Bind<IModelsExportService<FileModelObjectExportSettings>>()
            .To<NwcExportService>()
            .InSingletonScope();
        kernel.Bind<ILoggerService>()
            .ToMethod(c => PluginLoggerService)
            .InSingletonScope();
        kernel.Bind<IErrorsService>()
            .To<ErrorsService>()
            .InSingletonScope();
        kernel.Bind<INwcExportViewSettingsService>()
            .To<NwcExportViewSettingsService>()
            .InSingletonScope();
        kernel.Bind<RsModelObjectService>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<NwcExportViewSettingsViewModel>()
            .ToSelf()
            .InTransientScope();
        kernel.Bind<NwcExportViewSettingsDialog>()
            .ToSelf()
            .InTransientScope();
        kernel.Bind<IContentDialogService>()
            .To<ContentDialogService>()
            .InSingletonScope();

        kernel.Bind<IReadOnlyCollection<IServerClient>>()
            .ToMethod(c => c.Kernel.Get<Application>()
                .GetRevitServerNetworkHosts()
                .Select(item => new ServerClientBuilder()
                    .SetServerName(item)
                    .SetServerVersion(ModuleEnvironment.RevitVersion)
                    .Build())
                .ToArray());
        kernel.Bind<ViewModels.Rs.MainViewModel>().ToSelf();

        kernel.UseXtraOpenFolderDialog<FileSystemViewModel>(
            initialDirectory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
        kernel.UseXtraOpenFileDialog<NwcExportViewSettingsViewModel>(filter: "RVT (*.rvt)|*.rvt");

        kernel.BindMainWindow<FileSystemViewModel, MainWindow>();
        kernel.BindOtherWindow<ErrorsViewModel, ErrorsWindow>();

        kernel.UseWpfUIThemeUpdater();

        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        kernel.UseWpfLocalization($"/{assemblyName};component/assets/Localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        if(!OptionalFunctionalityUtils.IsNavisworksExporterAvailable()) {
            throw new InvalidOperationException(
                kernel.Get<ILocalizationService>().GetLocalizedString("NwcFromRvtWindow.PluginNotInstalled"));
        }

        Notification(kernel.Get<MainWindow>());
        if(kernel.Get<IErrorsService>().ContainsErrors()) {
            kernel.Get<ErrorsWindow>().ShowDialog();
        }
    }
}
