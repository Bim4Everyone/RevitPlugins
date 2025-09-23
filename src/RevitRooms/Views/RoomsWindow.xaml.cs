using System.Windows;

namespace RevitRooms.Views;
/// <summary>
/// Interaction logic for RoomsWindow.xaml
/// </summary>
public partial class RoomsWindow {
    public RoomsWindow() {
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
