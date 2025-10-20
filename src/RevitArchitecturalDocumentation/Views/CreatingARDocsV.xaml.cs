using System.Windows;

using dosymep.SimpleServices;

namespace RevitArchitecturalDocumentation.Views;
public partial class CreatingARDocsV {
    public CreatingARDocsV(
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
    public override string ProjectConfigName => nameof(CreatingARDocsV);

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
        Close();
    }
}
