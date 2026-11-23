using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.SimpleServices;

using Ninject;

using pyRevitLabs.Json;

using RevitLintelsManager.Models;
using RevitLintelsManager.Models.Configs;
using RevitLintelsManager.Models.Rules;
using RevitLintelsManager.Models.Settings;
using RevitLintelsManager.Services;
using RevitLintelsManager.ViewModels;
using RevitLintelsManager.Views;

using Wpf.Ui.Abstractions;

namespace RevitLintelsManager;

[Transaction(TransactionMode.Manual)]
public class RevitLintelsManagerCommand : BasePluginCommand {
    private static IManagerSession _session;

    /// <summary>Создаёт основную команду менеджера.</summary>
    public RevitLintelsManagerCommand() {
        PluginName = "Менеджер перемычек";
    }

    /// <summary>Проверяет настройки и передаёт владение контейнером немодальной сессии.</summary>
    protected override void Execute(UIApplication uiApplication) {
        if(_session is not null) {
            _session.Activate();
            return;
        }
        IKernel kernel = uiApplication.CreatePlatformServices();
        try {
            ConfigureDependencies(kernel);
            var messages = kernel.Get<WpfUIMessageBoxService>();
            var localization = kernel.Get<ILocalizationService>();
            if(uiApplication.ActiveUIDocument is null || uiApplication.ActiveUIDocument.Document.IsFamilyDocument) {
                ShowSettingsWarning(messages, localization, new[] { "Manager.Validation.ProjectRequired" });
                return;
            }
            var settings = kernel.Get<IPluginConfigRepository>().LoadSettings();
            var resolver = kernel.Get<ILintelManagerConfigResolver>();
            var config = resolver.BuildLintelManagerConfig(settings);
            var errors = kernel.Get<ILintelManagerConfigValidator>().ValidateDetailed(config);
            if(errors.Count > 0) {
                ShowSettingsWarning(messages, localization, errors);
                return;
            }
            var snapshot = resolver.CreateSnapshot(config);
            var repository = kernel.Get<RevitRepository>();
            var state = repository.CaptureInitialState(snapshot);
            repository.ReleaseCatalogs();
            kernel.Bind<ManagerSessionState>().ToConstant(state);
            kernel.Bind<WindowPlatformEvents>().ToSelf().InSingletonScope();
            kernel.Bind<Dispatcher>().ToConstant(Dispatcher.CurrentDispatcher);
#if LINTELS_DEMO
            kernel.Bind<ManagerDemoSource>().ToSelf().InSingletonScope();
            kernel.Bind<IManagerRequestDispatcher>()
                .ToMethod(context => context.Kernel.Get<ManagerDemoSource>());
#else
            kernel.Bind<IManagerRequestDispatcher>().To<ManagerExternalEventHandler>().InSingletonScope();
#endif
            kernel.Bind<ManagerDetailsWindow>().ToSelf();
            kernel.Bind<Func<ManagerDetailsWindow>>().ToMethod(context => {
                var owner = context.Kernel;
                return () => owner.Get<ManagerDetailsWindow>();
            });
            kernel.BindMainWindow<ManagerViewModel, ManagerWindow>();
            kernel.Bind<IManagerSession>().To<ManagerSession>().InSingletonScope();
#if LINTELS_DEMO
            kernel.Get<ManagerDemoSource>().Connect(kernel.Get<ManagerViewModel>(),
                kernel.Get<ISettingsCommandFactory>());
#endif
            var session = kernel.Get<IManagerSession>();
            session.Released += OnSessionReleased;
            session.Start(kernel);
            _session = session;
            kernel = null;
        } catch(OperationCanceledException) {
            // Закрытие при запуске не требует сообщения об ошибке.
        } catch(Exception exception) {
            kernel.Get<ILoggerService>().Warning(exception, "Ошибка запуска менеджера перемычек.");
            string message = IsInvalidSettingsJson(exception)
                ? "Manager.Validation.InvalidSettingsJson" : "Manager.StartFailed";
            ShowSettingsWarning(kernel.Get<WpfUIMessageBoxService>(), kernel.Get<ILocalizationService>(),
                new[] { message });
        } finally {
            kernel?.Dispose();
        }
    }

    // Распознаёт ошибку десериализации и при обёртке исключения контейнером DI.
    private static bool IsInvalidSettingsJson(Exception exception) {
        for(var cause = exception; cause is not null; cause = cause.InnerException) {
            if(cause is JsonException) {
                return true;
            }
        }
        return false;
    }

    // Регистрирует только общие используемые зависимости; главное окно выбирает вызывающая команда.
    internal static void ConfigureDependencies(IKernel kernel) {
        SettingsPlatformServices.Configure(kernel);
        kernel.Bind<RevitRepository>().ToSelf().InSingletonScope();
        kernel.Bind<IParamService>().To<ParamService>().InSingletonScope();
        kernel.Bind<PluginConfig>()
            .ToMethod(context => PluginConfig.GetPluginConfig(context.Kernel.Get<IConfigSerializer>()))
            .InSingletonScope();
        kernel.Bind<SystemPluginConfig>().ToSelf().InSingletonScope();
        kernel.Bind<ILintelConfigRuleStorage>().To<LintelConfigRuleStorage>().InSingletonScope();
        kernel.Bind<ILintelConfigRuleSerializer>().To<LintelConfigRuleSerializer>().InSingletonScope();
        kernel.Bind<ILintelConfigRuleResolver>().To<LintelConfigRuleResolver>().InSingletonScope();
        kernel.Bind<IPluginConfigRepository>().To<PluginConfigRepository>().InSingletonScope();
        kernel.Bind<ILintelManagerConfigResolver>().To<LintelManagerConfigResolver>().InSingletonScope();
        kernel.Bind<ILintelManagerConfigValidator>().To<LintelManagerConfigValidator>().InSingletonScope();
        kernel.Bind<ISettingsCommandFactory>().To<SettingsCommandFactory>().InSingletonScope();
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        kernel.UseWpfLocalization($"/{assemblyName};component/assets/Localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));
    }

    // Регистрирует модальные настройки с единственной загрузкой исходных данных.
    internal static void ConfigureSettingsDependencies(IKernel kernel) {
        kernel.Bind<INavigationViewPageProvider>().To<NavigationViewPageProvider>().InSingletonScope();
        kernel.Bind<ISettingsDialogService>().To<SettingsDialogService>().InSingletonScope();
        kernel.Bind<SettingsLaunchContext>().ToMethod(context => {
            var repository = context.Kernel.Get<IPluginConfigRepository>();
            var saved = repository.LoadSettings();
            return new SettingsLaunchContext(saved ?? repository.GetDefaultLintelManagerSettings(), saved is not null);
        }).InSingletonScope();
        kernel.Bind<LintelManagerSettings>().ToMethod(context => context.Kernel.Get<SettingsLaunchContext>().Settings);
        kernel.Bind<FamilySettingsViewModel>().ToSelf().InSingletonScope()
            .WithConstructorArgument("hasSavedSettings",
                context => context.Kernel.Get<SettingsLaunchContext>().HasSavedSettings);
        kernel.Bind<GeneralSettingsViewModel>().ToSelf().InSingletonScope();
        kernel.Bind<PlacementSettingsViewModel>().ToSelf().InSingletonScope();
        kernel.BindOtherWindow<NameInputViewModel, NameInputWindow>();
        kernel.BindMainWindow<SettingsViewModel, SettingsWindow>();
    }

    // Показывает локализованные причины отказа и предлагает отдельную команду настроек.
    private static void ShowSettingsWarning(IMessageBoxService messages, ILocalizationService localization,
        System.Collections.Generic.IEnumerable<string> errors) {
        string reasons = string.Join(Environment.NewLine, errors.Select(localization.GetLocalizedString));
        messages.Show(reasons + Environment.NewLine + Environment.NewLine
            + localization.GetLocalizedString("Manager.SettingsCommandHint"),
            localization.GetLocalizedString("RevitLintelsManagerCommand.SettingsCheckNecessaryMessageTitle"),
            MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    // Удаляет единственную ссылку после фактического освобождения сессии.
    private static void OnSessionReleased() {
        _session = null;
    }
}
