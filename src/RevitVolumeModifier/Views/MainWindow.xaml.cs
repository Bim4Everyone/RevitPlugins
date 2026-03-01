using dosymep.SimpleServices;

namespace RevitVolumeModifier.Views;

public partial class MainWindow {
    public MainWindow(
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

    public override string PluginName => nameof(RevitVolumeModifier);

    public override string ProjectConfigName => nameof(MainWindow);
}
