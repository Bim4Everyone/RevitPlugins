using dosymep.SimpleServices;

namespace RevitRooms.Views;
/// <summary>
/// Interaction logic for InfoElementsWindow.xaml
/// </summary>
public partial class WarningsWindow {
    public WarningsWindow(ILoggerService loggerService,
                              ISerializationService serializationService,
                              ILanguageService languageService, ILocalizationService localizationService,
                              IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService)
                : base(loggerService,
                       serializationService,
                       languageService, localizationService,
                       uiThemeService, themeUpdaterService) {
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitRooms);
    public override string ProjectConfigName => nameof(WarningsWindow);
}
