using System.Windows;

namespace RevitRooms.Views;
/// <summary>
/// Interaction logic for RoomsNums.xaml
/// </summary>
public partial class RoomsNumsWindows {
    public RoomsNumsWindows() {
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitRooms);
    public override string ProjectConfigName => nameof(RoomsNumsWindows);

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
