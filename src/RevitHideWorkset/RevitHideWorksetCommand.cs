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

using RevitHideWorkset.Models;
using RevitHideWorkset.ViewModels;
using RevitHideWorkset.Views;

namespace RevitHideWorkset;

/// <summary>
/// Класс команды Revit плагина.
/// </summary>
/// <remarks>
/// В данном классе должна быть инициализация контейнера плагина и указание названия команды.
/// </remarks>
[Transaction(TransactionMode.Manual)]
public class RevitHideWorksetCommand : BasePluginCommand {
    /// <summary>
    /// Инициализирует команду плагина.
    /// </summary>
    public RevitHideWorksetCommand() {
        PluginName = "Скрытие РН связей";
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

        kernel.UseWpfUIMessageBox();

        // Настройка запуска окна
        kernel.BindMainWindow<MainViewModel, MainWindow>();

        // Настройка локализации,
        // получение имени сборки откуда брать текст
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        // Настройка локализации,
        // установка дефолтной локализации "ru-RU"
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        ValidateLinkedFiles(kernel);

        // Вызывает стандартное уведомление
        Notification(kernel.Get<MainWindow>());
    }

    private void ValidateLinkedFiles(IKernel kernel) {
        var revitRepository = kernel.Get<RevitRepository>();
        var localizationService = kernel.Get<ILocalizationService>();
        var msg = kernel.Get<IMessageBoxService>();

        var linkedFiles = revitRepository.GetLinkedFiles();

        if(linkedFiles.Count == 0) {
            string title = localizationService.GetLocalizedString("GeneralSettings.ErrorMessage");
            string message = localizationService.GetLocalizedString("GeneralSettings.ErrorNoLinkedFiles");

            msg.Show(
                message,
                title
            );

            throw new OperationCanceledException();
        }
    }
}
