using System.Windows;

using dosymep.SimpleServices;

namespace RevitMarkAllDocuments.Views;

/// <summary>
/// Interaction logic for MarkListWindow.xaml
/// </summary>
public partial class MarkListWindow {
    public MarkListWindow(ILoggerService loggerService,
                          ISerializationService serializationService,
                          ILanguageService languageService,
                          ILocalizationService localizationService,
                          IUIThemeService uiThemeService,
                          IUIThemeUpdaterService themeUpdaterService) :
        base(loggerService,
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
