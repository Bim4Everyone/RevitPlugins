using System.Windows;

using dosymep.SimpleServices;

namespace RevitRooms.Views;
/// <summary>
/// Interaction logic for RoomsNums.xaml
/// </summary>
public partial class RoomsNumsWindow {
    public RoomsNumsWindow(
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
    public override string ProjectConfigName => nameof(RoomsNumsWindow);

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
