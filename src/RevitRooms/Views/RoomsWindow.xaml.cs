using System.Windows;

using dosymep.SimpleServices;

namespace RevitRooms.Views;
/// <summary>
/// Interaction logic for RoomsWindow.xaml
/// </summary>
public partial class RoomsWindow {
    public RoomsWindow(
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

    public override string PluginName => nameof(RevitRooms);
    public override string ProjectConfigName => nameof(RoomsWindow);

    private void ButtonOK_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
