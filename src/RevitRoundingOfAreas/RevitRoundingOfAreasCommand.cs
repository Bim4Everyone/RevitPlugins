using System;
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

using RevitRoundingOfAreas.Models;
using RevitRoundingOfAreas.Models.Interfaces;
using RevitRoundingOfAreas.ViewModels;
using RevitRoundingOfAreas.ViewModels.Warnings;
using RevitRoundingOfAreas.Views;

namespace RevitRoundingOfAreas;

/// <summary>
/// Класс команды Revit плагина.
/// </summary>
/// <remarks>
/// В данном классе должна быть инициализация контейнера плагина и указание названия команды.
/// </remarks>
[Transaction(TransactionMode.Manual)]
public class RevitRoundingOfAreasCommand : BasePluginCommand {
    /// <summary>
    /// Инициализирует команду плагина.
    /// </summary>
    public RevitRoundingOfAreasCommand() {
        PluginName = "Рассчитать площади (соц. объекты)";
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

        // Создание системных настроек
        kernel.Bind<SystemPluginConfig>()
            .ToSelf()
            .InSingletonScope();

        // Настройка доступа к Revit
        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();

        // Настройка доступа к классу параметров
        kernel.Bind<ParamService>()
            .ToSelf()
            .InSingletonScope();

        // Настройка доступа к классу проверок
        kernel.Bind<SpatialElementCheckService>()
            .ToSelf()
            .InSingletonScope();

        // Настройка доступа к сервису по управлению окнами
        kernel.Bind<IWindowService>()
            .To<WindowService>()
            .InSingletonScope();

        // Настройка конфигурации плагина
        kernel.Bind<PluginConfig>()
            .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

        // Настройка сервиса окошек сообщений
        kernel.UseWpfUIMessageBox<MainViewModel>();

        // Используем сервис обновления тем для WinUI
        kernel.UseWpfUIThemeUpdater();

        // Настройка запуска окна
        kernel.BindMainWindow<MainViewModel, MainWindow>();

        // Настройка запуска окна предупреждений
        kernel.BindOtherWindow<WarningsViewModel, WarningsWindow>();

        // Настройка локализации,
        // получение имени сборки откуда брать текст
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        // Настройка локализации,
        // установка дефолтной локализации "ru-RU"
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        var messageBoxService = kernel.Get<IMessageBoxService>();
        var localizationService = kernel.Get<ILocalizationService>();
        var systemPluginConfig = kernel.Get<SystemPluginConfig>();

        // Загрузка параметров проекта        
        bool isParamChecked = new CheckProjectParams(
            systemPluginConfig, uiApplication.Application, uiApplication.ActiveUIDocument.Document)
            .CopyProjectParams()
            .GetIsChecked();

        if(!isParamChecked) {
            messageBoxService.Show(
                localizationService.GetLocalizedString("Common.ParamErrorMessageBody"),
                localizationService.GetLocalizedString("Common.ConfigErrorMessageTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Exclamation);
            throw new OperationCanceledException();
        }

        // Вызывает стандартное уведомление
        Notification(kernel.Get<MainWindow>());
    }
}
