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

using RevitEnumerateBySpline.Models;
using RevitEnumerateBySpline.Models.Factories;
using RevitEnumerateBySpline.Models.Interfaces;
using RevitEnumerateBySpline.Models.Providers;
using RevitEnumerateBySpline.Models.Services;
using RevitEnumerateBySpline.ViewModels;
using RevitEnumerateBySpline.ViewModels.Warnings;
using RevitEnumerateBySpline.Views;

using Wpf.Ui.Abstractions;

namespace RevitEnumerateBySpline;

/// <summary>
/// Класс команды Revit плагина.
/// </summary>
/// <remarks>
/// В данном классе должна быть инициализация контейнера плагина и указание названия команды.
/// </remarks>
[Transaction(TransactionMode.Manual)]
public class RevitEnumerateBySplineCommand : BasePluginCommand {
    /// <summary>
    /// Инициализирует команду плагина.
    /// </summary>
    public RevitEnumerateBySplineCommand() {
        PluginName = "Нумератор по линии";
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
        
        // Настройка доступа к ProvidersFactory
        kernel.Bind<ProvidersFactory>()
            .ToSelf()
            .InSingletonScope();
        
        // Настройка доступа к классу параметров
        kernel.Bind<IParamService>()
            .To<ParamService>()
            .InSingletonScope();

        kernel.Bind<ISplineDividerService>()
            .To<SplineDividerService>()
            .InSingletonScope();

        kernel.Bind<ISplineSpatialIntersectionService>()
            .To<SplineSpatialIntersectionService>()
            .InSingletonScope();

        kernel.Bind<IEnumerateBySpline>()
            .To<EnumerateBySpline>()
            .InSingletonScope();

        kernel.Bind<IParamSetter>()
            .To<ParamSetter>()
            .InSingletonScope();

        kernel.Bind<ISpatialElementCheckService>()
            .To<SpatialElementCheckService>()
            .InSingletonScope();
        
        // Настройка доступа к RangeViewModel
        kernel.Bind<RangeViewModel>()
            .ToSelf()
            .InSingletonScope();
        
        // Настройка доступа к SpatialModelsViewModel
        kernel.Bind<SpatialModelsViewModel>()
            .ToSelf()
            .InSingletonScope();
        
        // Настройка доступа к CurveModelsViewModel
        kernel.Bind<CurveModelsViewModel>()
            .ToSelf()
            .InSingletonScope();
        
        // Настройка доступа к CommonParamSettingsViewModel
        kernel.Bind<CommonParamSettingsViewModel>()
            .ToSelf()
            .InSingletonScope();
        
        // Настройка доступа к ParkingSpaceParamSettingsViewModel
        kernel.Bind<ParkingSpaceParamSettingsViewModel>()
            .ToSelf()
            .InSingletonScope();

        // Настройка конфигурации плагина
        kernel.Bind<PluginConfig>()
            .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));
        
        // Настройка доступа к провайдеру по навигации страниц
        kernel.Bind<INavigationViewPageProvider>()
            .To<NavigationViewPageProvider>()
            .InSingletonScope();
        
        // Настройка доступа к классу параметров
        kernel.Bind<IWindowService>()
            .To<WindowService>()
            .InSingletonScope();
        
        // Настройка доступа к классу мессенджера
        kernel.Bind<IMessenger>()
            .To<Messenger>()
            .InSingletonScope();
        
        // Настройка доступа к RevitPickService
        kernel.Bind<ISelectionService>()
            .To<SelectionService>()
            .InSingletonScope();

        // Используем сервис обновления тем для WinUI
        kernel.UseWpfUIThemeUpdater();

        // Настройка запуска окна
        kernel.BindMainWindow<MainViewModel, MainWindow>();
        kernel.BindOtherWindow<WarningsViewModel, WarningsWindow>();

        // Настройка локализации,
        // получение имени сборки откуда брать текст
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        // Настройка локализации,
        // установка дефолтной локализации "ru-RU"
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        Notification(kernel.Get<MainWindow>());
    }
}
