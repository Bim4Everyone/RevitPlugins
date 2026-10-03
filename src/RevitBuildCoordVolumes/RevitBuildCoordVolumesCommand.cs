using System;
using System.Globalization;
using System.Reflection;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
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
        
        kernel.Bind<Document>()
            .ToMethod(_ => uiApplication.ActiveUIDocument.Document)
            .InSingletonScope();

        // Настройка доступа к Revit
        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();
        
        // Настройка доступа к сервису документов
        kernel.Bind<IDocumentService>()
            .To<DocumentService>()
            .InSingletonScope();

        // Настройка доступа к сервису управления окнами
        kernel.Bind<IWindowService>()
            .To<WindowService>()
            .InSingletonScope();
        
        // Настройка доступа к сервису разбития зон на элементы
        kernel.Bind<ISpatialElementDividerService>()
            .To<SpatialElementDividerService>()
            .InSingletonScope();
        
        // Настройка доступа к сервису плит перекрытий
        kernel.Bind<ISlabService>()
            .To<SlabService>()
            .InSingletonScope();
        
        // Настройка доступа к сервису нормализации плит перекрытий
        kernel.Bind<ISlabNormalizeService>()
            .To<SlabNormalizeService>()
            .InSingletonScope();
        
        // Настройка доступа к фабрике производства колонн
        kernel.Bind<IColumnFactory>()
            .To<ColumnFactory>()
            .InSingletonScope();
        
        // Настройка доступа к сервису контуров
        kernel.Bind<IContourService>()
            .To<ContourService>()
            .InSingletonScope();
        
        // Настройка доступа к фабрике геометрических объектов
        kernel.Bind<IGeomObjectFactory>()
            .To<GeomObjectFactory>()
            .InSingletonScope();
        
        // Настройка доступа к сервису установки параметров
        kernel.Bind<IParamSetter>()
            .To<ParamSetter>()
            .InSingletonScope();
        
        // Настройка доступа к фабрике объектов DirectShape
        kernel.Bind<IDirectShapeObjectFactory>()
            .To<DirectShapeObjectFactory>()
            .InSingletonScope();
        
        // Настройка доступа к сервису доступности категорий
        kernel.Bind<ICategoryAvailabilityService>()
            .To<CategoryAvailabilityService>()
            .InSingletonScope();
        
        // Настройка доступа к сервису доступности параметров
        kernel.Bind<IParamAvailabilityService>()
            .To<ParamAvailabilityService>()
            .InSingletonScope();
        
        // Настройка доступа к сервису проверки зон
        kernel.Bind<ISpatialElementCheckService>()
            .To<SpatialElementCheckService>()
            .InSingletonScope();
        
        // Настройка доступа к классу коннектора геометрических объектов
        kernel.Bind<IGeomObjectConnector>()
            .To<GeomObjectConnector>()
            .InSingletonScope();
        
        // Настройка доступа к сервису построения геометрических объектов
        kernel.Bind<IGeomObjectsBuildService>()
            .To<GeomObjectsBuildService>()
            .InSingletonScope();
        
        // Настройка доступа к сервису получения геометрии плит
        kernel.Bind<ISlabGeometryService>()
            .To<SlabGeometryService>()
            .InSingletonScope();

        // Настройка доступа к агрегатору сервисов
        kernel.Bind<BuildCoordVolumeServices>()
            .ToSelf()
            .InSingletonScope();
        
        // Настройка доступа к сервису анализатора плит
        kernel.Bind<ISlabAnalyzeSlopeService>()
            .To<SlabAnalyzeSlopeService>()
            .InSingletonScope();

        // Настройка конфигурации плагина
        kernel.Bind<PluginConfig>()
            .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

        kernel.UseWpfUIProgressDialog<MainViewModel>();

        // Используем сервис обновления тем для WinUI
        kernel.UseWpfUIThemeUpdater();

        // Настройка сервиса окошек сообщений
        kernel.UseWpfUIMessageBox<MainViewModel>();

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
