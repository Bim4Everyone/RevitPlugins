using dosymep.SimpleServices;

namespace RevitPylonLoadAreas.Views;

public partial class ErrorsWindow {
    public ErrorsWindow(
        ILoggerService loggerService,
        ISerializationService serializationService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IUIThemeService uiThemeService,
        IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService,
            serializationService,
            languageService,
            localizationService,
            uiThemeService,
            themeUpdaterService) {
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitPylonLoadAreas);

    public override string ProjectConfigName => nameof(ErrorsWindow);
}
