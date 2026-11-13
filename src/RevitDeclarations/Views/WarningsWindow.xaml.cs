using System.Windows;

using dosymep.SimpleServices;

namespace RevitDeclarations.Views;
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

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
