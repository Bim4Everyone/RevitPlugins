using System;
using System.Globalization;
using System.Reflection;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Serializers;
using dosymep.SimpleServices;
using dosymep.WPF.Views;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;
using dosymep.Xpf.Core.Ninject;

using Ninject;

using RevitLintelsManager.Models;
using RevitLintelsManager.Models.Configs;
using RevitLintelsManager.Models.Rules;
using RevitLintelsManager.ViewModels;
using RevitLintelsManager.Views;

using Wpf.Ui.Abstractions;

namespace RevitLintelsManager;

[Transaction(TransactionMode.Manual)]
public class RevitLintelsManagerCommand : BasePluginCommand {
    
    public RevitLintelsManagerCommand() {
        PluginName = "Менеджер перемычек";
    }
    
    protected override void Execute(UIApplication uiApplication) {
        // Создание контейнера зависимостей плагина с сервисами из платформы
        using IKernel kernel = uiApplication.CreatePlatformServices();
        
        // Настройка доступа к провайдеру по навигации страниц
        kernel.Bind<INavigationViewPageProvider>()
            .To<NavigationViewPageProvider>()
            .InSingletonScope();

        // Настройка доступа к Revit
        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();
        
        // Настройка конфигурации плагина
        kernel.Bind<PluginConfig>()
            .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));
        
        // Настройка доступа к классу с основными константами плагина
        kernel.Bind<SystemPluginConfig>()
            .ToSelf()
            .InSingletonScope();
        
        // Настройка доступа к классу с настройками правил
        kernel.Bind<LintelConfigRuleStorage>()
            .ToSelf()
            .InSingletonScope();
        
        // Настройка доступа к сериализатору настроек правил
        kernel.Bind<LintelConfigRuleSerializer>()
            .ToSelf()
            .InSingletonScope();
        
        // Настройка доступа к валидатору настроек правил
        kernel.Bind<LintelConfigRuleValidator>()
            .ToSelf()
            .InSingletonScope();
        
        // Настройка доступа к резолверу настроек правил
        kernel.Bind<LintelConfigRuleResolver>()
            .ToSelf()
            .InSingletonScope();
        
        // Настройка доступа к классу с основными настройками плагина
        kernel.Bind<PluginConfigRepository>()
            .ToSelf()
            .InSingletonScope();
        
        // Настройка доступа к классу с настройками менеджера перемычек
        kernel.Bind<LintelManagerConfigResolver>()
            .ToSelf()
            .InSingletonScope();
        
        // Настройка доступа к классу с валидатором настроек менеджера перемычек
        kernel.Bind<LintelManagerConfigValidator>()
            .ToSelf()
            .InSingletonScope();

        // Используем сервис обновления тем для WinUI
        kernel.UseWpfUIThemeUpdater();

        // Настройка запуска окна
        kernel.BindMainWindow<SettingsViewModel, SettingsWindow>();

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
        
        var pluginConfigRepository = kernel.Get<PluginConfigRepository>();
        var lintelManagerSettings = pluginConfigRepository.LoadSettings();
        
        if (lintelManagerSettings is null) {
            ShowSettingsWarningWindow(messageBoxService, localizationService);
            throw new OperationCanceledException();
        }
        
        var lintelManagerConfigResolver = kernel.Get<LintelManagerConfigResolver>();
        var lintelManagerConfig = lintelManagerConfigResolver.BuildLintelManagerConfig(lintelManagerSettings);
        
        var lintelManagerConfigValidator = kernel.Get<LintelManagerConfigValidator>();

        if(!lintelManagerConfigValidator.Validate(lintelManagerConfig)) {
            ShowSettingsWarningWindow(messageBoxService, localizationService);
            throw new OperationCanceledException();       
        }

        // Вызывает стандартное уведомление
        Notification(kernel.Get<SettingsWindow>());
    }
    
    private static void ShowSettingsWarningWindow( IMessageBoxService messageBoxService, ILocalizationService localizationService) {
        messageBoxService.Show(
            localizationService.GetLocalizedString("RevitLintelsManagerCommand.SettingsCheckNecessaryMessageBody"),
            localizationService.GetLocalizedString("RevitLintelsManagerCommand.SettingsCheckNecessaryMessageTitle"),
            MessageBoxButton.OK,
            MessageBoxImage.Exclamation);
    }
}
