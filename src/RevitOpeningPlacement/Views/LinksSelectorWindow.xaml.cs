using System.Windows;

namespace RevitOpeningPlacement.Views;
public partial class LinksSelectorWindow {
    public LinksSelectorWindow() {
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitOpeningPlacement);
    public override string ProjectConfigName => nameof(LinksSelectorWindow);

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
