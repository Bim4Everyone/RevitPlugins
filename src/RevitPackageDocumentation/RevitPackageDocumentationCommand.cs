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

using pyRevitLabs.Json;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.Models.ConfigSerializer;
using RevitPackageDocumentation.ViewModels;
using RevitPackageDocumentation.ViewModels.Configuration;
using RevitPackageDocumentation.Views;

namespace RevitPackageDocumentation;

/// <summary>
/// Класс команды Revit плагина.
/// </summary>
[Transaction(TransactionMode.Manual)]
public class RevitPackageDocumentationCommand : BasePluginCommand {
    /// <summary>
    /// Инициализирует команду плагина.
    /// </summary>
    public RevitPackageDocumentationCommand() {
        PluginName = "RevitPackageDocumentation";
    }

    /// <summary>
    /// Метод выполнения основного кода плагина.
    /// </summary>
    /// <param name="uiApplication">Интерфейс взаимодействия с Revit.</param>
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

        kernel.Bind<IRevitElementPickerService>()
            .ToMethod(c => RevitElementPickerService.GetRevitElementPickerService(
                c.Kernel.Get<RevitRepository>(),
                c.Kernel.Get<MainWindow>(),
                c.Kernel.Get<ILocalizationService>()))
            .InSingletonScope();

        // Сервис открытия диалогового окна сохранения/открытия файла JSON
        kernel.Bind<IFileDialogService>()
            .To<JsonFileDialogService>()
            .InSingletonScope();

        // Регистрация коллекции конвертеров JSON
        kernel.Bind<JsonConverter>()
            .To<SheetComponentConverter>()
            .InSingletonScope();

        kernel.Bind<JsonConverter>()
            .To<PluginParamConverter>()
            .InSingletonScope();

        // JSON сериализатор с получением всех зарегистрированных конвертеров
        kernel.Bind<ISheetSetSerializer>()
            .ToMethod(ctx => {
                var converters = ctx.Kernel.GetAll<JsonConverter>();
                return new SheetSetSerializer(converters);
            })
            .InSingletonScope();

        // Настройка конфигурации комплекта листов
        kernel.Bind<SheetSetConfig>()
            .ToSelf()
            .InSingletonScope();

        // Фабрика по созданию VM из JSON DTO
        kernel.Bind<ISheetSetVMFactory>()
            .To<SheetSetVMFactory>()
            .InSingletonScope();

        // Фабрика по созданию JSON DTO из VM 
        kernel.Bind<ISheetSetDataFactory>()
            .To<SheetSetDataFactory>()
            .InSingletonScope();

        // Сервис обновления свойств по параметрам конфигурации
        kernel.Bind<StringParamSetService>()
            .ToSelf()
            .InSingletonScope();

        // Настройка сервиса окошек сообщений
        kernel.UseWpfUIMessageBox<MainViewModel>();

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
