using System.Windows;

using dosymep.SimpleServices;

namespace RevitReinforcementCoefficient.Views;
public partial class ReportWindow {
    public ReportWindow(ILoggerService loggerService,
        ISerializationService serializationService,
        ILanguageService languageService, ILocalizationService localizationService,
        IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService,
            serializationService,
            languageService, localizationService,
            uiThemeService, themeUpdaterService) {
        InitializeComponent();
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonForHide_Click(object sender, RoutedEventArgs e) {
        Hide();
    }
}
