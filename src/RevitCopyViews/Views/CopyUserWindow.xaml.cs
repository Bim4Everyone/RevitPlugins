using System.Windows;

namespace RevitCopyViews.Views;

/// <summary>
///     Interaction logic for CopyUserView.xaml
/// </summary>
public partial class CopyUserWindow {
    public CopyUserWindow() {
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitCopyViews);
    public override string ProjectConfigName => nameof(CopyUserWindow);

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
