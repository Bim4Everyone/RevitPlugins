using dosymep.SimpleServices;

namespace RevitSleeves.Views.Placing;
public partial class PlacingErrorsWindow {
    public PlacingErrorsWindow(
        ILoggerService loggerService,
        ISerializationService serializationService,
        ILanguageService languageService, ILocalizationService localizationService,
        IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService,
            serializationService,
            languageService, localizationService,
            uiThemeService, themeUpdaterService) {
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitSleeves);

    public override string ProjectConfigName => nameof(PlacingErrorsWindow);
}
