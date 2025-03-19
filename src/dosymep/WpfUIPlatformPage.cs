using System;
using System.Globalization;
using System.Windows.Controls;

using dosymep.SimpleServices;

namespace dosymep.WpfUI.Core {
    public class WpfUIPlatformPage : Page, IHasTheme, IHasLocalization {
        public event Action<UIThemes> ThemeChanged;
        public event Action<CultureInfo> LanguageChanged;

        public WpfUIPlatformPage() { }

        public WpfUIPlatformPage(
            ILoggerService loggerService,
            ILanguageService languageService, ILocalizationService localizationService,
            IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService) {
            LoggerService = loggerService;

            LanguageService = languageService;
            LocalizationService = localizationService;

            UIThemeService = uiThemeService;
            ThemeUpdaterService = themeUpdaterService;

            UIThemeService.UIThemeChanged += _ => ThemeChanged?.Invoke(_);
            LanguageService.LanguageChanged += _ => LanguageChanged?.Invoke(_);
        }

        public ILoggerService LoggerService { get; }

        public ILanguageService LanguageService { get; }
        public ILocalizationService LocalizationService { get; }

        public IUIThemeService UIThemeService { get; }
        public IUIThemeUpdaterService ThemeUpdaterService { get; }

        public UIThemes HostTheme => UIThemeService.HostTheme;
        public CultureInfo HostLanguage => LanguageService.HostLanguage;
    }
}
