using dosymep.SimpleServices;

namespace RevitFinishingWalls.Views {
    public partial class ErrorWindow {
        public ErrorWindow(
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


        public override string PluginName => nameof(RevitFinishingWalls);
        public override string ProjectConfigName => nameof(ErrorWindow);
    }
}
