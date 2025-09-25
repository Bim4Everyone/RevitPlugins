using System.Windows;

namespace RevitSuperfilter.Views;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow {
    public MainWindow() {
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitSuperfilter);
    public override string ProjectConfigName => nameof(MainWindow);

    private void ButtonOK_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
