using System;
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
using RevitPackageDocumentation.Views.Extensions;

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
        PluginName = "Документация комплекта";
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

        // Сервис работы с библиотекой семейств на диске (типовые аннотации из папки)
        kernel.Bind<FamilyLibraryService>()
            .ToSelf()
            .InSingletonScope();

        // Сервис обновления свойств по параметрам конфигурации
        kernel.Bind<StringParamSetService>()
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

        // Инициализируем extension для локализации ресурсов в файле ресурсов (без доступа к ресурсам окна)
        var localizationService = kernel.Get<ILocalizationService>();
        LocalizedExtension.Init(localizationService);

        // Настройка сервиса окошек сообщений
        kernel.UseWpfUIMessageBox<MainViewModel>();

        // Сервисы открытия диалогового окна сохранения/открытия файла JSON
        kernel.UseWpfOpenFileDialog<MainViewModel>(
            filter: "JSON files (*.json)|*.json|All files (*.*)|*.*",
            title: localizationService.GetLocalizedString("MainViewModel.SelectConfigurationFile"));
        kernel.UseWpfSaveFileDialog<MainViewModel>(
            filter: "JSON files (*.json)|*.json",
            title: localizationService.GetLocalizedString("MainViewModel.SaveConfiguration"),
            defaultFileName: "config.json",
            addExtension: true);

        // Сервис выбора папки с семействами (передается фабрикой в модули типовых аннотаций)
        kernel.UseWpfOpenFolderDialog<SheetSetVMFactory>(
            title: localizationService.GetLocalizedString("MainViewModel.SelectFamilyFolder"),
            initialDirectory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            multiSelect: false);

        // Окно только собирает настройки. Листы создаются после закрытия окна:
        // между закрытием окна и работой с документом нужно освободить COM-объекты окна
        // (см. ReleaseClosedWindowComObjects)
        var window = kernel.Get<MainWindow>();
        bool? dialogResult = window.ShowDialog();
        if(dialogResult == true && window.DataContext is MainViewModel mainViewModel) {
            ReleaseClosedWindowComObjects();
            mainViewModel.ProcessSheets();
        }

        // Вызывает стандартное уведомление
        Notification(dialogResult);
    }

    /// <summary>
    /// Освобождает COM-объекты закрытого окна плагина до начала работы с документом.
    /// </summary>
    /// <remarks>
    /// Защита от зависания Revit 2024 при загрузке семейств предыдущих версий (например, 2022).
    /// <para>
    /// Поля ввода WPF создают в главном потоке Revit COM-объекты службы ввода текста Windows (TSF).
    /// После закрытия окна их .NET-обертки становятся мусором, но сами объекты освобождаются финализатором,
    /// который для этого обращается в главный поток (COM STA) и ждет его ответа.
    /// </para>
    /// <para>
    /// После каждой внешней команды Revit сам вызывает GC.Collect и GC.WaitForPendingFinalizers,
    /// и главный поток ждет финализатор. Если внутри команды семейство обновлялось до текущей версии
    /// (Revit показывает служебные окна обновления), цикл ожидания главного потока может перестать
    /// обрабатывать входящие вызовы COM. Тогда финализатор ждет главный поток, а главный поток - финализатор:
    /// Revit зависает. Это подтверждено дампами процесса.
    /// </para>
    /// <para>
    /// Поэтому объекты закрытого окна освобождаются здесь, до загрузки и обновления семейств, пока главный
    /// поток еще обрабатывает вызовы COM. К сборке мусора Revit после команды освобождать в главном потоке
    /// уже нечего. Метод должен вызываться после закрытия окна и до <see cref="MainViewModel.ProcessSheets"/>;
    /// вызывать его после загрузки семейств нельзя - он сам может зависнуть по той же причине.
    /// </para>
    /// </remarks>
    private static void ReleaseClosedWindowComObjects() {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}
