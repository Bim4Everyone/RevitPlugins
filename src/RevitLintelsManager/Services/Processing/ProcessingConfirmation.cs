using System.Globalization;
using System.Windows;

using dosymep.SimpleServices;
using dosymep.WpfUI.Core.SimpleServices;

namespace RevitLintelsManager.Services;

internal interface IProcessingConfirmation {
    /// <summary>Запрашивает согласие на указанные количества удаления и замены.</summary>
    /// <param name="deleteCount">Число согласуемых удалений.</param>
    /// <param name="replaceCount">Число согласуемых замен.</param>
    /// <returns>Пользователь подтвердил выполнение.</returns>
    bool Show(int deleteCount, int replaceCount);
}

/// <summary>Корпоративный диалог с собственными подписями кнопок в пределах одного подтверждения.</summary>
internal sealed class ProcessingConfirmation(IUIThemeService theme, ILanguageService language,
    IUIThemeUpdaterService updater, ILocalizationService localization) : IProcessingConfirmation {
    /// <inheritdoc />
    public bool Show(int deleteCount, int replaceCount) {
        var context = new SettingsDialogContext(theme, language, updater, new ButtonLocalization(localization));
        var dialog = new WpfUIMessageBoxService(context, context) { AllowAttach = false };
        return dialog.Show(localization.GetLocalizedString("Processing.Confirm", deleteCount, replaceCount),
            localization.GetLocalizedString("Manager.Title"), MessageBoxButton.OKCancel, MessageBoxImage.Warning,
            MessageBoxResult.Cancel) == MessageBoxResult.OK;
    }

    // Dosymep запрашивает подписи по фиксированным ключам. Переадресация действует только в этом диалоге.
    private sealed class ButtonLocalization(ILocalizationService source) : ILocalizationService {
        /// <inheritdoc />
        public string GetLocalizedString(string key) => source.GetLocalizedString(GetResourceKey(key));

        /// <inheritdoc />
        public string GetLocalizedString(string key, params object[] args) =>
            source.GetLocalizedString(GetResourceKey(key), args);

        /// <inheritdoc />
        public void SetLocalization(CultureInfo culture) => source.SetLocalization(culture);

        /// <inheritdoc />
        public void SetLocalization(CultureInfo culture, FrameworkElement element) =>
            source.SetLocalization(culture, element);

        // Берёт команду подтверждения и существующую подпись отмены из общего словаря текущего языка.
        private static string GetResourceKey(string key) => key switch {
            "MessageBox.Ok" => "Processing.Continue",
            "MessageBox.Cancel" => "MainWindow.ButtonCancel",
            _ => key
        };
    }
}
