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

using RevitRoomAnnotations.Models;
using RevitRoomAnnotations.Services;
using RevitRoomAnnotations.ViewModels;
using RevitRoomAnnotations.Views;

namespace RevitRoomAnnotations;

/// <summary>
/// Класс команды Revit плагина.
/// </summary>
/// <remarks>
/// В данном классе должна быть инициализация контейнера плагина и указание названия команды.
/// </remarks>
[Transaction(TransactionMode.Manual)]
public class RevitRoomAnnotationsCommand : BasePluginCommand {
    /// <summary>
    /// Инициализирует команду плагина.
    /// </summary>
    public RevitRoomAnnotationsCommand() {
        PluginName = "Обновить помещения в ЭОМ";
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

        // Настройка конфигурации плагина
        kernel.Bind<PluginConfig>()
            .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

        // Используем сервис обновления тем для WinUI
        kernel.UseWpfUIThemeUpdater();

        // Настройка запуска окна
        kernel.BindMainWindow<MainViewModel, MainWindow>();

        kernel.Bind<IRoomAnnotationMapService>()
            .To<RoomAnnotationMapService>()
            .InSingletonScope();

        // Настройка локализации,
        // получение имени сборки откуда брать текст
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        // Настройка локализации,
        // установка дефолтной локализации "ru-RU"
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        Validation(kernel);
        // Вызывает стандартное уведомление
        Notification(kernel.Get<MainWindow>());
    }

    private void Validation(IKernel kernel) {
        var revitRepository = kernel.Get<RevitRepository>();
        var localizationService = kernel.Get<ILocalizationService>();

        var linkedFiles = revitRepository.GetLinkedFiles();

        if(linkedFiles.Count == 0) {
            string title = localizationService.GetLocalizedString("GeneralSettings.ErrorMessage");
            string message = localizationService.GetLocalizedString("GeneralSettings.ErrorNoSelectedElements");

            _ = TaskDialog.Show(title, message);
            throw new OperationCanceledException();
        }

        var annotation = revitRepository.GetRoomAnnotationSymbol();

        if(annotation == null) {
            string title = localizationService.GetLocalizedString("GeneralSettings.ErrorMessage");
            string message = localizationService.GetLocalizedString("GeneralSettings.ErrorAnnotationTypeNotFound");

            _ = TaskDialog.Show(title, message);
            throw new OperationCanceledException();
        }
    }
}
