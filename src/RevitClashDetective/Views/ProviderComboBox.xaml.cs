using dosymep.SimpleServices;

namespace RevitClashDetective.Views;
public partial class ProviderComboBox {

    public ProviderComboBox() : base() {
        InitializeComponent();
    }

    public ProviderComboBox(
        ILoggerService loggerService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IUIThemeService uiThemeService,
        IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService,
            languageService,
            localizationService,
            uiThemeService,
            themeUpdaterService) {
        InitializeComponent();
    }
}
