using dosymep.SimpleServices;

namespace RevitEnumerateBySpline.Views;

public partial class WarningsWindow {
    public WarningsWindow(
        ILoggerService loggerService,
        ISerializationService serializationService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IUIThemeService uiThemeService,
        IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService, serializationService, languageService,
            localizationService, uiThemeService, themeUpdaterService) {
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitEnumerateBySpline);
    public override string ProjectConfigName => nameof(WarningsWindow);
}
