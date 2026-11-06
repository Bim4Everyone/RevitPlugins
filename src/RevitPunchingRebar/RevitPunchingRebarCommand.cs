using System.Globalization;
using System.Reflection;
using System.Text;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WPF.Views;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;
using dosymep.Xpf.Core.Ninject;

using Ninject;

using RevitPunchingRebar.Models;
using RevitPunchingRebar.Services;
using RevitPunchingRebar.ViewModels;
using RevitPunchingRebar.Views;

namespace RevitPunchingRebar;

/// <summary>
/// Класс команды Revit плагина.
/// </summary>
/// <remarks>
/// В данном классе должна быть инициализация контейнера плагина и указание названия команды.
/// </remarks>
[Transaction(TransactionMode.Manual)]
public class RevitPunchingRebarCommand : BasePluginCommand {
    /// <summary>
    /// Инициализирует команду плагина.
    /// </summary>
    public RevitPunchingRebarCommand() {
        PluginName = "Поперечное армирование";
    }

    /// <summary>
    /// Метод выполнения основного кода плагина.
    /// </summary>
    /// <param name="uiApplication">Интерфейс взаимодействия с Revit.</param>
    /// <remarks>
    /// В случаях, когда не используется конфигурация
    /// или локализация требуется удалять их использование полностью во всем проекте.
    /// </remarks>
    protected override void Execute(UIApplication uiApplication) {
        // Создание контейнера зависимостей плагина с сервисами из платформы
        using IKernel kernel = uiApplication.CreatePlatformServices();

        // Настройка доступа к Revit
        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();

        // Настройка конфигурации плагина
        kernel.Bind<PluginConfig>()
            .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

        // Сервис расстановки каркасов
        kernel.Bind<IPunchingRebarPlacementService>()
            .To<PunchingRebarPlacementService>()
            .InSingletonScope();

        // Используем сервис обновления тем для WinUI
        kernel.UseWpfUIThemeUpdater();

        // Настройка запуска окна
        kernel.BindMainWindow<MainViewModel, MainWindow>();

        // Настройка локализации,
        // получение имени сборки откуда брать текст
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        // Настройка локализации,
        // установка дефолтной локализации "ru-RU"
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        // Вызывает стандартное уведомление
        MainWindow window = kernel.Get<MainWindow>();
        Notification(window);

        ShowPlacementReport((MainViewModel) window.DataContext, kernel.Get<ILocalizationService>());
    }

    /// <summary>
    /// Показывает итог расстановки: сколько пилонов обработано, сколько каркасов создано и какие пилоны пропущены.
    /// </summary>
    private void ShowPlacementReport(MainViewModel viewModel, ILocalizationService localizationService) {
        PlacementResult result = viewModel.PlacementResult;
        if(result == null) {
            return;
        }

        StringBuilder report = new StringBuilder();
        report.AppendFormat(
            localizationService.GetLocalizedString("Report.Summary"),
            result.ProcessedPylonsCount,
            result.PylonsCount,
            result.CreatedFramesCount);

        if(result.SkippedPylons.Count > 0) {
            report.AppendLine();
            report.AppendLine();
            report.AppendLine(localizationService.GetLocalizedString("Report.SkippedPylons"));
            foreach(string skippedPylon in result.SkippedPylons) {
                report.AppendLine(skippedPylon);
            }
        }

        TaskDialog.Show(PluginName, report.ToString().TrimEnd());
    }
}
