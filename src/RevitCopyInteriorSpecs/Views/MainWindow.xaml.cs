using System.Windows;

namespace RevitCopyInteriorSpecs.Views;
public partial class MainWindow {
    public MainWindow() {
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitCopyInteriorSpecs);
    public override string ProjectConfigName => nameof(MainWindow);

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
