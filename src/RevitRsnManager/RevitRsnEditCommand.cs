using System.Globalization;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;
using dosymep.Xpf.Core.Ninject;

using Ninject;

using RevitRsnManager.Interfaces;
using RevitRsnManager.Models;
using RevitRsnManager.ViewModels;
using RevitRsnManager.Views;

namespace RevitRsnManager;

/// <summary>
/// Класс команды Revit плагина.
/// </summary>
/// <remarks>
/// В данном классе должна быть инициализация контейнера плагина и указание названия команды.
/// </remarks>
[Transaction(TransactionMode.Manual)]
public class RevitRsnEditCommand : BasePluginCommand {
    /// <summary>
    /// Инициализирует команду плагина.
    /// </summary>
    public RevitRsnEditCommand() {
        PluginName = "Менеджер файла RSN.ini";
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
        using var kernel = uiApplication.CreatePlatformServices();

        // Настройка доступа к Revit
        _ = kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();

        // Настройка конфигурации плагина
        _ = kernel.Bind<PluginConfig>()
            .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

        _ = kernel.Bind<IRsnConfigService>()
            .To<RsnConfigService>()
            .InSingletonScope();

        // Используем сервис обновления тем для WinUI
        _ = kernel.UseWpfUIThemeUpdater();

        _ = kernel.UseWpfUIMessageBox();

        // Настройка запуска окна
        _ = kernel.BindMainWindow<MainViewModel, MainWindow>();

        // Настройка локализации,
        // получение имени сборки откуда брать текст
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        // Настройка локализации,
        // установка дефолтной локализации "ru-RU"
        _ = kernel.UseXtraLocalization(
            $"/{assemblyName};component/assets/localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        // Вызывает стандартное уведомление
        Notification(kernel.Get<MainWindow>());
    }
}
