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

using RevitAreaBoundaries.Models;
using RevitAreaBoundaries.Models.Processors;
using RevitAreaBoundaries.Services;
using RevitAreaBoundaries.ViewModels;
using RevitAreaBoundaries.Views;

using Wpf.Ui.Abstractions;

namespace RevitAreaBoundaries;

/// <summary>
/// Класс команды Revit плагина.
/// </summary>
/// <remarks>
/// В данном классе должна быть инициализация контейнера плагина и указание названия команды.
/// </remarks>
[Transaction(TransactionMode.Manual)]
public class RevitAreaBoundariesCommand : BasePluginCommand {
    /// <summary>
    /// Инициализирует команду плагина.
    /// </summary>
    public RevitAreaBoundariesCommand() {
        PluginName = "Границы зон ТЭП";
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
        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();
        
        // Создание системных настроек
        kernel.Bind<SystemPluginConfig>()
            .ToSelf()
            .InSingletonScope();
        
        // Настройка доступа к сервису получения ограничивающего квадрата
        kernel.Bind<IOuterSquareService>()
            .To<OuterSquareService>()
            .InSingletonScope();
        
        // Настройка доступа к сервису по разделению линий на мелкие отрезки
        kernel.Bind<ICurveDividerService>()
            .To<CurveDividerService>()
            .InSingletonScope();
        
        // Настройка доступа к сервису по разделению линий на мелкие отрезки
        kernel.Bind<ICurveNormalizeService>()
            .To<CurveNormalizeService>()
            .InSingletonScope();
        
        // Настройка доступа к сервису по получению кривых сечения
        kernel.Bind<IElementSectionService>()
            .To<ElementSectionService>()
            .InSingletonScope();
        
        // Настройка доступа к сервису по получению границ ячеек
        kernel.Bind<ICellsBoundaryService>()
            .To<CellsBoundaryService>()
            .InSingletonScope();
        
        // Настройка доступа к сервису по исправлению кривых
        kernel.Bind<ICurveRepairService>()
            .To<CurveRepairService>()
            .InSingletonScope();
        
        // Настройка доступа к сервису по рисованию границ
        kernel.Bind<IDrawBoundaryService>()
            .To<DrawBoundaryService>()
            .InSingletonScope();
        
        // Настройка доступа к сервису по восстановлению коротких кривых в длинные
        kernel.Bind<ICollinearLineMergeService>()
            .To<CollinearLineMergeService>()
            .InSingletonScope();
        
        // Настройка доступа к сервису по соединению свободных концов
        kernel.Bind<IFreeEndsJoinService>()
            .To<FreeEndsJoinService>()
            .InSingletonScope();
        
        // Настройка доступа к сервису BoundingBox
        kernel.Bind<IBoundingBoxService>()
            .To<BoundingBoxService>()
            .InSingletonScope();
        
        // Настройка доступа к основному процессору
        kernel.Bind<IBoundaryDrawer>()
            .To<OutBoundaryDrawer>()
            .InSingletonScope();
           
        // Настройка доступа к провайдеру по навигации страниц
        kernel.Bind<INavigationViewPageProvider>()
            .To<NavigationViewPageProvider>()
            .InSingletonScope();

        // Настройка конфигурации плагина
        kernel.Bind<PluginConfig>()
            .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));
        
        // Используем фабрику прогресс-бара
        kernel.UseWpfUIProgressDialog<MainViewModel>();

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
        Notification(kernel.Get<MainWindow>());
    }
}
