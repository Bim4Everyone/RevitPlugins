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

using RevitExportSpecToJson.Models;
using RevitExportSpecToJson.Services;
using RevitExportSpecToJson.ViewModels;
using RevitExportSpecToJson.Views;

namespace RevitExportSpecToJson;

/// <summary>
/// Класс команды Revit плагина.
/// </summary>
/// <remarks>
/// В данном классе должна быть инициализация контейнера плагина и указание названия команды.
/// </remarks>
[Transaction(TransactionMode.Manual)]
public class RevitExportSpecToJsonCommand : BasePluginCommand {
    /// <summary>
    /// Инициализирует команду плагина.
    /// </summary>
    public RevitExportSpecToJsonCommand() {
        PluginName = "Спеки в JSON";
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



        var localization = kernel.Get<ILocalizationService>();

        kernel.UseWpfUIProgressDialog<MainViewModel>(
            stepValue: 1,
            displayTitleFormat: localization.GetLocalizedString("ProgressDialog.Title"));

        kernel.UseWpfOpenFolderDialog<MainViewModel>(
            title: localization.GetLocalizedString("OpenFolderDialog.Title"),
            initialDirectory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
        
        
        kernel.Bind<ISaveToJsonService>()
            .To<SaveToJsonService>()
            .InTransientScope();

        // Вызывает стандартное уведомление
        Notification(kernel.Get<MainWindow>());
    }
}
