using dosymep.SimpleServices;

namespace RevitArchitecturalDocumentation.Views;
/// <summary>
/// Логика взаимодействия для TreeReportV.xaml
/// </summary>
public partial class TreeReportV {
    public TreeReportV(
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

    public override string PluginName => nameof(RevitArchitecturalDocumentation);
    public override string ProjectConfigName => nameof(TreeReportV);
}
