using System.Windows.Controls;

using dosymep.SimpleServices;

using RevitPlatformSettings.ViewModels.Settings;

namespace RevitPlatformSettings.Views.Pages {
    internal partial class ExtensionsSettingsPage {
        public ExtensionsSettingsPage() { }

        public ExtensionsSettingsPage(ExtensionsSettingsViewModel viewModel,
            ILoggerService loggerService,
            ILanguageService languageService, ILocalizationService localizationService,
            IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService)
            : base(loggerService,
                languageService, localizationService,
                uiThemeService, themeUpdaterService) {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}

