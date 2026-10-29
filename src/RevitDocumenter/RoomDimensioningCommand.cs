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

using RevitDocumenter.Models;
using RevitDocumenter.Models.Dimensions.DimensionServices;
using RevitDocumenter.Models.Mapping.ViewServices;
using RevitDocumenter.Models.RoomDimensions;
using RevitDocumenter.Models.RoomDimensions.Contours;
using RevitDocumenter.Models.RoomDimensions.Debug;
using RevitDocumenter.Models.RoomDimensions.Planning;
using RevitDocumenter.Models.RoomDimensions.Settings;
using RevitDocumenter.ViewModels;
using RevitDocumenter.Views;

namespace RevitDocumenter;

/// <summary>
/// Команда образмеривания помещений на активном плане.
/// </summary>
[Transaction(TransactionMode.Manual)]
public class RoomDimensioningCommand : BasePluginCommand {
    public RoomDimensioningCommand() {
        PluginName = "Размеры помещений";
    }

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

        // Чтение JSON-файла настроек образмеривания
        kernel.Bind<RoomDimensioningSettingsReader>()
            .ToSelf()
            .InSingletonScope();

        // Образмеривание: контуры, привязки, пробные размеры, журнал
        kernel.Bind<DimensionCreator>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<ContourNormalizer>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<RoomContourBuilder>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<ReferenceResolver>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<ContourDimensionPlanner>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<VirtualDimensionBuilder>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<DebugDumpWriter>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<AnchorLineService>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<ContourDebugDrawer>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<ViewImageExporter>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<RoomDimensioningService>()
            .ToSelf()
            .InSingletonScope();

        // Используем сервис обновления тем для WinUI
        kernel.UseWpfUIThemeUpdater();

        // Настройка запуска окна
        kernel.BindMainWindow<RoomDimensioningViewModel, RoomDimensioningWindow>();

        // Настройка локализации
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        // Диалог выбора JSON-файла настроек
        var localizationService = kernel.Get<ILocalizationService>();
        kernel.UseWpfOpenFileDialog<RoomDimensioningViewModel>(
            filter: "JSON files (*.json)|*.json|All files (*.*)|*.*",
            title: localizationService.GetLocalizedString("RoomDimensioningWindow.SelectSettingsFile"));

        // Вызывает стандартное уведомление
        Notification(kernel.Get<RoomDimensioningWindow>());
    }
}
