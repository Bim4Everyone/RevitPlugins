using System.Windows;

namespace RevitCopyViews.Views;

/// <summary>
///     Interaction logic for CopyViewWindow.xaml
/// </summary>
public partial class CopyViewWindow {
    public CopyViewWindow() {
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitCopyViews);
    public override string ProjectConfigName => nameof(CopyViewWindow);

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
