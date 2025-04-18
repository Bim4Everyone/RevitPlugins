using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WPF.Views;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;
using dosymep.Xpf.Core.Ninject;

using Ninject;

using RevitRsnManager.Interfaces;
using RevitRsnManager.Models;
using RevitRsnManager.ViewModels;
using RevitRsnManager.Views;

namespace RevitRsnManager;

/// <summary>
/// Класс команды Revit плагина.
/// </summary>
/// <remarks>
/// В данном классе должна быть инициализация контейнера плагина и указание названия команды.
/// </remarks>
[Transaction(TransactionMode.Manual)]
public class RevitRsnLoadCommand : BasePluginCommand {
    /// <summary>
    /// Инициализирует команду плагина.
    /// </summary>
    public RevitRsnLoadCommand() {
        PluginName = "RevitRsnManager";
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

        kernel.Bind<IRsnConfigService>()
            .To<RsnConfigService>()
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
        kernel.UseXtraLocalization(
            $"/{assemblyName};component/Localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        var pluginConfig = kernel.Get<PluginConfig>();
        var revitRepository = kernel.Get<RevitRepository>();
        var rsnConfigService = kernel.Get<RsnConfigService>();
        var localizationService = kernel.Get<ILocalizationService>();

        var config = pluginConfig.GetSettings(revitRepository.Document);
        
        if(config != null && config.Servers?.Any() == true) {
            var updateSuccess = localizationService.GetLocalizedString("MainWindow.UpdateSuccess");
            var updateSuccessTitle = localizationService.GetLocalizedString("MainWindow.UpdateSuccessTitle");

            rsnConfigService.SaveServersToIni(config.Servers);
            MessageBox.Show(
               updateSuccess,
               updateSuccessTitle,
               MessageBoxButton.OK,
               MessageBoxImage.Information
           );
        } else {
            var configNotFound = localizationService.GetLocalizedString("MainWindow.ConfigNotFound");
            var configNotFoundTitle = localizationService.GetLocalizedString("MainWindow.ConfigNotFoundTitle");

            MessageBox.Show(
                configNotFound,
                configNotFoundTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            Notification(kernel.Get<MainWindow>());
        }
    }
}
