using System.Windows;

namespace RevitRefreshLinks.Views;
public partial class DirectoriesExplorerWindow {
    public DirectoriesExplorerWindow() {
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitRefreshLinks);
    public override string ProjectConfigName => nameof(DirectoriesExplorerWindow);

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
