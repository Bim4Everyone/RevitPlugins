using System;

using System.Windows;

using dosymep.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitLintelsManager.Views;

using Wpf.Ui;

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

        kernel.Bind<IContentDialogService>().To<ContentDialogService>().InSingletonScope();
    }

    public static void ShowSettingsWindow(IKernel kernel) {
        try {
            kernel.Get<SettingsWindow>().ShowDialog();
        } catch(OperationCanceledException) {
            // Отмена не должна обращаться к глобальному Xpf-сервису уведомлений.
        } catch(Exception exception) {
            kernel.Get<ILoggerService>().Warning(exception, "Ошибка окна настроек перемычек.");
            // Диалог не зависит от ресурсов окна и контекста активации Revit TaskDialog.
            MessageBox.Show(exception.Message,
                kernel.Get<ILocalizationService>().GetLocalizedString("Common.Error"),
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
