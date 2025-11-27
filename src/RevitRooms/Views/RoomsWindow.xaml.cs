using System.Windows;

using dosymep.SimpleServices;

using RevitRooms.ViewModels;

using Wpf.Ui.Controls;

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

    private void CheckBox_Checked(object sender, RoutedEventArgs e) {
        ChangeSelected(true);
    }

    private void CheckBox_Unchecked(object sender, RoutedEventArgs e) {
        ChangeSelected(false);
    }

    private void ChangeSelected(bool state) {
        var dataGrid = (DataGrid) FindName("Levels");
        var levels = dataGrid.SelectedItems;
        foreach(LevelViewModel level in levels) {
            level.IsSelected = state;
        }
    }
}
