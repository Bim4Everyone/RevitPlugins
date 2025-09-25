using System.Windows;

using dosymep.WPF.Views;

namespace RevitMarkPlacement.Views;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : PlatformWindow {
    public MainWindow() {
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitMarkPlacement);
    public override string ProjectConfigName => nameof(MainWindow);

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
