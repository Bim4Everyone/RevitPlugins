using System;
using System.Globalization;
using System.Windows.Controls;

using dosymep.SimpleServices;
using dosymep.WpfCore.Behaviors;

using Microsoft.Xaml.Behaviors;

namespace dosymep.WpfUI.Core {
    public class WpfUIPlatformUserControl : UserControl, IHasTheme, IHasLocalization {
        public event Action<UIThemes> ThemeChanged;
        public event Action<CultureInfo> LanguageChanged;

        public WpfUIPlatformUserControl() { }

        public WpfUIPlatformUserControl(
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
            
            Interaction.GetBehaviors(this).Add(new WpfThemeBehavior());
            Interaction.GetBehaviors(this).Add(new WpfLocalizationBehavior());
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
