using System;
using System.Globalization;

using dosymep.SimpleServices;

namespace RevitLintelsManager.Services;

/// <summary>Отсоединяемые publishers для анонимных подписок платформенного окна.</summary>
public sealed class WindowPlatformEvents : ILanguageService, IUIThemeService, IDisposable {
    private readonly ILanguageService _language;
    private readonly IUIThemeService _theme;
    private CultureInfo _hostLanguage;
    private UIThemes _hostTheme;
    private bool _isDisposed;

    /// <summary>Создаёт WindowPlatformEvents с переданными зависимостями.</summary>
    public WindowPlatformEvents(ILanguageService language, IUIThemeService theme) {
        _language = language;
        _theme = theme;
        _hostLanguage = language.HostLanguage;
        _hostTheme = theme.HostTheme;
        _language.LanguageChanged += OnLanguageChanged;
        _theme.UIThemeChanged += OnThemeChanged;
    }

    public event Action<CultureInfo> LanguageChanged;
    public event Action<UIThemes> UIThemeChanged;
    public CultureInfo HostLanguage => _hostLanguage;
    public UIThemes HostTheme => _hostTheme;

    /// <inheritdoc />
    public void Dispose() {
        if(_isDisposed) {
            return;
        }
        _isDisposed = true;
        _language.LanguageChanged -= OnLanguageChanged;
        _theme.UIThemeChanged -= OnThemeChanged;
        LanguageChanged = null;
        UIThemeChanged = null;
    }

    // Передаёт смену языка только живому окну.
    private void OnLanguageChanged(CultureInfo language) {
        _hostLanguage = language;
        LanguageChanged?.Invoke(language);
    }

    // Передаёт смену темы только живому окну.
    private void OnThemeChanged(UIThemes theme) {
        _hostTheme = theme;
        UIThemeChanged?.Invoke(theme);
    }
}
