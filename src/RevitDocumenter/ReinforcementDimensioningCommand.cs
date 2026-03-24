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

using RevitDocumenter.Extensions;
using RevitDocumenter.Models;
using RevitDocumenter.Models.Dimensions.DimensionLines;
using RevitDocumenter.Models.Dimensions.DimensionReferences;
using RevitDocumenter.Models.Dimensions.DimensionReferences.ReferenceCollector;
using RevitDocumenter.Models.Dimensions.DimensionServices;
using RevitDocumenter.ViewModels;
using RevitDocumenter.Views;

namespace RevitDocumenter;

/// <summary>
/// Класс команды Revit плагина.
/// </summary>
/// <remarks>
/// В данном классе должна быть инициализация контейнера плагина и указание названия команды.
/// </remarks>
[Transaction(TransactionMode.Manual)]
public class ReinforcementDimensioningCommand : BasePluginCommand {
    /// <summary>
    /// Инициализирует команду плагина.
    /// </summary>
    public ReinforcementDimensioningCommand() {
        PluginName = "Доп.Арм. Размеры";
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
        kernel.BindMapping();

        // Настройка доступа к Revit
        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();

        // Настройка конфигурации плагина
        kernel.Bind<PluginConfig>()
            .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

        kernel.Bind<IReferenceCollector<ReferenceToGridsCollectorContext>>()
            .To<ReferenceToGridsCollector>()
            .InSingletonScope();

        kernel.Bind<ReferenceAnalizeService>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<GridDirectionFilter>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<IDimensionLineProvider<RebarElementDimensionLineProviderContext>>()
            .To<RebarElementDimensionLineProvider>()
            .InSingletonScope();

        kernel.Bind<DimensionCreator>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<DimensionChanger>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<ValueGuard>()
            .ToSelf()
            .InSingletonScope();

        // Используем сервис обновления тем для WinUI
        kernel.UseWpfUIThemeUpdater();

        // Настройка запуска окна
        kernel.BindMainWindow<ReinforcementDimensioningViewModel, ReinforcementDimensioningWindow>();

        // Настройка локализации,
        // получение имени сборки откуда брать текст
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        // Настройка локализации,
        // установка дефолтной локализации "ru-RU"
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        // Вызывает стандартное уведомление
        Notification(kernel.Get<ReinforcementDimensioningWindow>());
    }
}
