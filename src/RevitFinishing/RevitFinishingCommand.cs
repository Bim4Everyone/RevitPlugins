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

using RevitFinishing.Models;
using RevitFinishing.Models.Finishing;
using RevitFinishing.Services;
using RevitFinishing.ViewModels;
using RevitFinishing.ViewModels.Notices;
using RevitFinishing.Views;

namespace RevitFinishing;

/// <summary>
/// Класс команды Revit плагина.
/// </summary>
/// <remarks>
/// В данном классе должна быть инициализация контейнера плагина и указание названия команды.
/// </remarks>
[Transaction(TransactionMode.Manual)]
public class RevitFinishingCommand : BasePluginCommand {
    /// <summary>
    /// Инициализирует команду плагина.
    /// </summary>
    public RevitFinishingCommand() {
        PluginName = "Рассчитать отделку";
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

        kernel.Bind<ErrorWindowService>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<ProjectValidationService>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<FinishingValidationService>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<ParamCalculationService>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<FinishingFloorFactory>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<FinishingWallFactory>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<FinishingCeilingFactory>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<FinishingBaseboardFactory>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<FinishingInProject>()
            .ToSelf()
            .InSingletonScope();

        // Настройка конфигурации плагина
        kernel.Bind<PluginConfig>()
            .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

        // Используем сервис обновления тем для WinUI
        kernel.UseWpfUIThemeUpdater();

        // Настройка запуска окна
        kernel.BindMainWindow<MainViewModel, MainWindow>();
        kernel.BindOtherWindow<ErrorsViewModel, ErrorsWindow>();

        // Настройка локализации,
        // получение имени сборки откуда брать текст
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        // Настройка локализации,
        // установка дефолтной локализации "ru-RU"
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        // Вызывает стандартное уведомление
        Notification(kernel.Get<MainWindow>());
    }
}
