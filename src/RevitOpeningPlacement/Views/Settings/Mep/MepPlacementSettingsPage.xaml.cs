using System.Windows;

using dosymep.SimpleServices;

namespace RevitOpeningPlacement.Views.Settings.Mep;
public partial class MepPlacementSettingsPage {
    public MepPlacementSettingsPage() {
        InitializeComponent();
    }

    public MepPlacementSettingsPage(
        ILoggerService loggerService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IUIThemeService uiThemeService,
        IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService, languageService, localizationService, uiThemeService, themeUpdaterService) {
        InitializeComponent();
    }

    private void CheckFilter_Clicked(object sender, RoutedEventArgs e) {
        Window parent = Window.GetWindow((DependencyObject) sender);

        if(parent is not null) {
            parent.DialogResult = true;
        }
    }
}
