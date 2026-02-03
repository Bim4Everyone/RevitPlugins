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

using RevitBuildCoordVolumes.Models;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Services;
using RevitBuildCoordVolumes.ViewModels;
using RevitBuildCoordVolumes.Views;

namespace RevitBuildCoordVolumes;

/// <summary>
/// Класс команды Revit плагина.
/// </summary>
/// <remarks>
/// В данном классе должна быть инициализация контейнера плагина и указание названия команды.
/// </remarks>
[Transaction(TransactionMode.Manual)]
public class RevitBuildCoordVolumesCommand : BasePluginCommand {
    /// <summary>
    /// Инициализирует команду плагина.
    /// </summary>
    public RevitBuildCoordVolumesCommand() {
        PluginName = "Построение объемов СМР";
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

        // Создание сервиса по управлению окнами
        kernel.Bind<IWindowService>()
            .To<WindowService>()
            .InSingletonScope();

        // Создание сервисов
        kernel.Bind<BuildCoordVolumeServices>()
            .ToSelf()
            .InSingletonScope();

        // Настройка конфигурации плагина
        kernel.Bind<PluginConfig>()
            .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

        kernel.UseWpfUIProgressDialog<MainViewModel>();

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
            uiApplication.Application, uiApplication.ActiveUIDocument.Document, systemPluginConfig)
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
