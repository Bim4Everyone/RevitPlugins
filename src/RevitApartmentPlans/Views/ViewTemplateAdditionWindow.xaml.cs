using System.Windows;

using dosymep.SimpleServices;

namespace RevitApartmentPlans.Views;
public partial class ViewTemplateAdditionWindow {
    public ViewTemplateAdditionWindow(
    ILoggerService loggerService,
    ISerializationService serializationService,
    ILanguageService languageService,
    ILocalizationService localizationService,
    IUIThemeService uiThemeService,
    IUIThemeUpdaterService themeUpdaterService)
    : base(loggerService,
        serializationService,
        languageService, localizationService,
        uiThemeService, themeUpdaterService) {
        InitializeComponent();
    }


    public override string PluginName => nameof(RevitApartmentPlans);
    public override string ProjectConfigName => nameof(ViewTemplateAdditionWindow);

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
