using dosymep.SimpleServices;

namespace RevitSleeves.Views.Settings;
public partial class FilterWindow {
    public FilterWindow(
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

    public override string ProjectConfigName => nameof(FilterWindow);
}
