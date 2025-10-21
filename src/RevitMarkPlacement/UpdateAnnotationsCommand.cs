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

using RevitMarkPlacement.Models;
using RevitMarkPlacement.Models.DocumentProviders;
using RevitMarkPlacement.Models.SelectionModes;
using RevitMarkPlacement.ViewModels;
using RevitMarkPlacement.Views;

namespace RevitMarkPlacement;

/// <summary>
/// Класс команды Revit плагина.
/// </summary>
/// <remarks>
/// В данном классе должна быть инициализация контейнера плагина и указание названия команды.
/// </remarks>
[Transaction(TransactionMode.Manual)]
public class UpdateAnnotationsCommand : BasePluginCommand {
    /// <summary>
    /// Инициализирует команду плагина.
    /// </summary>
    public UpdateAnnotationsCommand() {
        PluginName = "Расстановщик отметок";
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
        kernel.BindMainWindow<MainViewModel, ReportWindow>();

        // Настройка локализации,
        // получение имени сборки откуда брать текст
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        // Настройка локализации,
        // установка дефолтной локализации "ru-RU"
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localizations/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        kernel.Bind<IDocumentProvider>()
            .To<ActiveDocumentProvider>()
            .InSingletonScope();

        kernel.Bind<ISpotDimensionSelection>()
            .To<DBSelection>()
            .InSingletonScope();

        kernel.Bind<ISpotDimensionSelection>()
            .To<DBViewSelection>()
            .InSingletonScope();

        kernel.Bind<ISpotDimensionSelection>()
            .To<SelectedOnViewSelection>()
            .InSingletonScope();

        kernel.Bind<IGlobalParamSelection>()
            .To<DoubleGlobalParamSelection>()
            .InSingletonScope();

        var viewModel = kernel.Get<MainViewModel>();
        if(!viewModel.CanPlaceAnnotation()) {
            // Вызывает стандартное уведомление
            Notification(kernel.Get<ReportWindow>());
        } else {
            RevitRepository revitRepository = kernel.Get<RevitRepository>();

            var marks = new TemplateLevelMarkCollection(
                revitRepository,
                new DBSelection(new ActiveDocumentProvider(uiApplication)));
            marks.UpdateAnnotation();
        }
    }
}
