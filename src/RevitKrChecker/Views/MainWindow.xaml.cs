using System.Windows;

namespace RevitKrChecker.Views;
public partial class MainWindow {
    public MainWindow() {
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitKrChecker);
    public override string ProjectConfigName => nameof(MainWindow);

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
