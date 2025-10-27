using System.Windows;
using System.Windows.Input;

using dosymep.SimpleServices;


namespace RevitArchitecturalDocumentation.Views;
public partial class CopySpecSheetInstanceV {
    public CopySpecSheetInstanceV(
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
    public override string ProjectConfigName => nameof(CopySpecSheetInstanceV);

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        Close();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e) {
        if(e.Key == Key.Escape) {
            Close();
        }
    }
}
