using System.Globalization;
using System.Reflection;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitClassifierParameters.Models;
using RevitClassifierParameters.Models.FacadeType;
using RevitClassifierParameters.Models.MaterialClassifier;
using RevitClassifierParameters.Models.Work;
using RevitClassifierParameters.ViewModels;
using RevitClassifierParameters.Views;

namespace RevitClassifierParameters;

/// <summary>
/// Класс команды Revit плагина.
/// </summary>
/// <remarks>
/// В данном классе должна быть инициализация контейнера плагина и указание названия команды.
/// </remarks>
[Transaction(TransactionMode.Manual)]
public class RevitArParameterClassifierCommand : BasePluginCommand {
    /// <summary>
    /// Инициализирует команду плагина.
    /// </summary>
    public RevitArParameterClassifierCommand() {
        PluginName = "RevitClassifierParameters";
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

        kernel.Bind<WorkGroupCode>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<MaterialParamSetter>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<MaterialReportService>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<ClassifierExcelReader>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<FacadeTypeExcelReader>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<FacadeTypeSetter>()
            .ToSelf()
            .InSingletonScope();

        // Настройка окна отчёта
        kernel.Bind<ReportVM>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<ReportV>()
            .ToSelf()
            .InSingletonScope()
            .WithPropertyValue(nameof(Window.DataContext),
                c => c.Kernel.Get<ReportVM>());

        // Используем сервис обновления тем для WPF UI
        kernel.UseWpfUIThemeUpdater();

        // Настройка запуска окна
        kernel.BindMainWindow<ArParameterClassifierVM, ArParameterClassifierV>();

        // Настройка локализации,
        // получение имени сборки откуда брать текст
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        // Настройка локализации,
        // установка дефолтной локализации "ru-RU"
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        // Настройка сервиса окошек сообщений
        kernel.UseWpfUIMessageBox<ArParameterClassifierVM>();

        // Сервис открытия диалогового окна для чтения файла Классификатора
        kernel.UseWpfOpenFileDialog<ArParameterClassifierVM>(
            filter: "Excel files (*.xlsx;*.xls)|*.xlsx;*.xls|All files (*.*)|*.*");

        // Вызывает стандартное уведомление
        Notification(kernel.Get<ArParameterClassifierV>());
    }
}
