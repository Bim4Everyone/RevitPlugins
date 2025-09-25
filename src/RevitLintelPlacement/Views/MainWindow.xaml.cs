using System.Windows;

namespace RevitLintelPlacement.Views;

/// <summary>
///     Логика взаимодействия для MainView.xaml
/// </summary>
public partial class MainWindow {
    public MainWindow() {
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitLintelPlacement);
    public override string ProjectConfigName => nameof(MainWindow);

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }

    private void SimpleButtonOK_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }
}
