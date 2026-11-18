using System;

using System.Windows;

using dosymep.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;
using dosymep.WpfUI.Core.SimpleServices;

using Ninject;

using RevitLintelsManager.ViewModels;
using RevitLintelsManager.Views;

namespace RevitLintelsManager.Services;

internal static class SettingsPlatformServices {
    public static void Configure(IKernel kernel) {
        // CreatePlatformServices регистрирует старые Xpf-сервисы по умолчанию.
        kernel.UseWpfWindowsTheme();
        kernel.UseWpfDispatcher();
        kernel.UseWpfOpenFileDialog();
        kernel.UseWpfSaveFileDialog();
        kernel.UseWpfUIMessageBox();
        kernel.UseWpfUINotifications(nameof(RevitLintelsManager));
        kernel.UseWpfUIThemeUpdater();

        kernel.BindOtherWindow<NameInputViewModel, NameInputWindow>();

        // Аварийный сервис не должен зависеть от создания основного окна.
        kernel.Bind<WpfUIMessageBoxService>().ToSelf()
            .WithConstructorArgument("theme", context => context.Kernel.Get<SettingsDialogContext>())
            .WithConstructorArgument("localization", context => context.Kernel.Get<SettingsDialogContext>())
            .WithPropertyValue("AllowAttach", false);
    }

    public static void ShowSettingsWindow(IKernel kernel) {
        try {
            kernel.Get<SettingsWindow>().ShowDialog();
        } catch(OperationCanceledException) {
            // Отмена не должна обращаться к глобальному Xpf-сервису уведомлений.
        } catch(Exception exception) {
            kernel.Get<ILoggerService>().Warning(exception, "Ошибка окна настроек перемычек.");
            kernel.Get<WpfUIMessageBoxService>().Show(exception.Message,
                kernel.Get<ILocalizationService>().GetLocalizedString("Common.Error"),
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
