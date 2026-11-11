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
using RevitEnumerateBySpline.Models.Handlers;
using RevitEnumerateBySpline.Models.Interfaces;
using RevitEnumerateBySpline.Models.Services;
using RevitEnumerateBySpline.Providers;
using RevitEnumerateBySpline.ViewModels;
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
        
        // Настройка доступа к ExternalRevitHandler
        kernel.Bind<ExternalRevitHandler>()
            .ToSelf()
            .InSingletonScope();

        // Настройка доступа к Revit
        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();
        
        // Настройка доступа к ProvidersFactory
        kernel.Bind<ProvidersFactory>()
            .ToSelf()
            .InSingletonScope();
        
        // Настройка доступа к классу параметров
        kernel.Bind<ParamService>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<SplineDividerService>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<SplineSpatialIntersectionService>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<IEnumerateBySpline>()
            .To<EnumerateBySpline>()
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
        kernel.Bind<WindowService>()
            .ToSelf()
            .InSingletonScope();
        
        // Настройка доступа к классу мессенджера
        kernel.Bind<IMessenger>()
            .To<Messenger>()
            .InSingletonScope();
        
        // Настройка доступа к RevitPickService
        kernel.Bind<SelectionService>()
            .ToSelf()
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

        ShowMainWindowLoop(kernel);
    }

    private void ShowMainWindowLoop(IKernel kernel) {
        var mainWindow = kernel.Get<MainWindow>();
        var mainViewModel = mainWindow.DataContext as MainViewModel
                            ?? kernel.Get<MainViewModel>();
        var curveModelsViewModel = mainViewModel.CurveModelsViewModel;
        var selectionService = kernel.Get<SelectionService>();
        var revitRepository = kernel.Get<RevitRepository>();
        var windowService = kernel.Get<WindowService>();

        windowService.SetMainWindow(mainWindow);

        while(true) {
            bool? dialogResult = mainWindow.ShowDialog();

            if(mainWindow.DialogAction == MainWindowAction.SelectCurves) {
                string prompt = kernel.Get<ILocalizationService>()
                    .GetLocalizedString("CurveModelsViewModel.SelectCurves.Promt");
                var curves = selectionService.PickCurve(prompt, revitRepository.ActiveUiDocument);
                curveModelsViewModel.AddCurves(curves);

                mainWindow = CreateMainWindow(kernel, mainViewModel);
                windowService.SetMainWindow(mainWindow);
                continue;
            }

            if(dialogResult == false) {
                GetPlatformService<INotificationService>()
                    .CreateWarningNotification(PluginName, "Выполнение скрипта отменено.")
                    .ShowAsync();
                return;
            }

            Notification(dialogResult);
            return;
        }
    }

    private static MainWindow CreateMainWindow(IKernel kernel, MainViewModel mainViewModel) {
        return new MainWindow(
            kernel.Get<INavigationViewPageProvider>(),
            kernel.Get<ILoggerService>(),
            kernel.Get<ISerializationService>(),
            kernel.Get<ILanguageService>(),
            kernel.Get<ILocalizationService>(),
            kernel.Get<IUIThemeService>(),
            kernel.Get<IUIThemeUpdaterService>()) {
            DataContext = mainViewModel
        };
    }
}
