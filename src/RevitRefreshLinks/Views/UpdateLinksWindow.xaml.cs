using System.Windows;

namespace RevitRefreshLinks.Views;
public partial class UpdateLinksWindow {
    public UpdateLinksWindow() {
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitRefreshLinks);
    public override string ProjectConfigName => nameof(UpdateLinksWindow);

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
