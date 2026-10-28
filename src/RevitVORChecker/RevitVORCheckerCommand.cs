using System.Globalization;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitVORChecker.Models;
using RevitVORChecker.Services;
using RevitVORChecker.ViewModels;
using RevitVORChecker.Views;

namespace RevitVORChecker;

/// <summary>
/// Команда «Проверка ВОР»: проверка BIM-моделей КР перед выгрузкой ВОР и реестр замечаний .docx.
/// </summary>
[Transaction(TransactionMode.Manual)]
public class RevitVORCheckerCommand : BasePluginCommand {
    public RevitVORCheckerCommand() {
        PluginName = "Проверка ВОР";
    }

    protected override void Execute(UIApplication uiApplication) {
        using IKernel kernel = uiApplication.CreatePlatformServices();

        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<CheckProcess>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<PluginConfig>()
            .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

        kernel.UseWpfUIThemeUpdater();

        kernel.BindMainWindow<MainViewModel, MainWindow>();
        kernel.BindOtherWindow<ProjectCardViewModel, ProjectCardWindow>();

        kernel.UseWpfUIProgressDialog<MainViewModel>();
        kernel.UseWpfUIMessageBox<MainViewModel>();
        kernel.UseWpfUIMessageBox<ProjectCardViewModel>();
        kernel.UseWpfOpenFolderDialog<MainViewModel>(
            multiSelect: false,
            initialDirectory: Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        kernel.UseWpfOpenFileDialog<MainViewModel>(
            filter: "Excel |*.xlsx",
            multiSelect: false,
            initialDirectory: Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        kernel.UseWpfOpenFileDialog<ProjectCardViewModel>(
            filter: "project.json |*.json",
            multiSelect: false,
            initialDirectory: Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/Localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        Notification(kernel.Get<MainWindow>());
    }
}
