using dosymep.SimpleServices;

namespace RevitSleeves.Views.Filtration;
public partial class StructureLinksSelectorWindow {
    public StructureLinksSelectorWindow(
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


    public override string PluginName => nameof(RevitSleeves);

    public override string ProjectConfigName => nameof(StructureLinksSelectorWindow);

    private void ButtonOk_Click(object sender, System.Windows.RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, System.Windows.RoutedEventArgs e) {
        DialogResult = false;
    }
}
