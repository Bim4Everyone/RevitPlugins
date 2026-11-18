using System;
using System.Globalization;

using dosymep.SimpleServices;

namespace RevitLintelsManager.Services;

internal class SettingsDialogContext(IUIThemeService themeService, ILanguageService languageService,
    IUIThemeUpdaterService themeUpdaterService, ILocalizationService localizationService)
    : IHasTheme, IHasLocalization {
    public event Action<UIThemes> ThemeChanged {
        add => themeService.UIThemeChanged += value;
        remove => themeService.UIThemeChanged -= value;
    }

    public event Action<CultureInfo> LanguageChanged {
        add => languageService.LanguageChanged += value;
        remove => languageService.LanguageChanged -= value;
    }

    public UIThemes HostTheme => themeService.HostTheme;
    public CultureInfo HostLanguage => languageService.HostLanguage;
    public IUIThemeUpdaterService ThemeUpdaterService => themeUpdaterService;
    public ILocalizationService LocalizationService => localizationService;
}
