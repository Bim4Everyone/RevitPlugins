using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
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
        kernel.Bind<CheckRunner>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<PluginConfig>()
            .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

        kernel.UseWpfUIThemeUpdater();

        kernel.BindMainWindow<MainViewModel, MainWindow>();
        kernel.BindOtherWindow<ProjectCardViewModel, ProjectCardWindow>();

        kernel.UseWpfUIMessageBox();
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

        // Окно показывается здесь, а не через Notification(window) шаблона: тот сразу после закрытия окна
        // выдаёт уведомление «завершено успешно», а проверка идёт после окна. Уведомление платформы —
        // после проверки (Notification(true)); при отмене окна Notification(false) завершает команду отменой.
        var mainWindow = kernel.Get<MainWindow>();
        bool? dialogResult = mainWindow.ShowDialog();
        if(dialogResult != true) {
            Notification(dialogResult);
            return;
        }

        CheckSettings settings = ((MainViewModel) mainWindow.DataContext).Settings
                                 ?? throw new InvalidOperationException("Check settings are not set.");
        CheckOutcome outcome = kernel.Get<CheckRunner>().Run(settings);
        ShowOutcome(outcome, kernel.Get<IMessageBoxService>(), kernel.Get<ILocalizationService>());
        if(outcome.Result != null) {
            Notification(true);
        }
    }

    /// <summary>
    /// Итог проверки: число замечаний и путь реестра (с предложением открыть папку), отмена или ошибка.
    /// </summary>
    private static void ShowOutcome(
        CheckOutcome outcome,
        IMessageBoxService messageBoxService,
        ILocalizationService localizationService) {

        string title = localizationService.GetLocalizedString("MainWindow.Title");
        if(outcome.Result is { } result) {
            string message = localizationService.GetLocalizedString("Result.Instruction", result.FindingCount)
                             + "\n\n" + localizationService.GetLocalizedString("Result.Registry")
                             + "\n" + result.RegistryPath
                             + "\n\n" + localizationService.GetLocalizedString("Result.OpenFolder");
            MessageBoxResult answer = messageBoxService.Show(
                message,
                title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Information,
                MessageBoxResult.Yes);
            if(answer == MessageBoxResult.Yes) {
                Process.Start("explorer.exe", "/select,\"" + result.RegistryPath + "\"");
            }

            return;
        }

        if(outcome.IsCancelled) {
            messageBoxService.Show(
                localizationService.GetLocalizedString("MainWindow.Cancelled"),
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        string errorMessage = localizationService.GetLocalizedString("MainWindow.Stopped")
                              + "\n" + outcome.Error?.GetBaseException().Message;
        if(outcome.ErrorReportPath is { } reportPath) {
            errorMessage += "\n\n" + localizationService.GetLocalizedString("MainWindow.ErrorReport") + "\n" + reportPath;
        }

        messageBoxService.Show(errorMessage, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
