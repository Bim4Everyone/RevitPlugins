using System.Windows;
using System.Windows.Input;

namespace RevitCopyViews.Views;

/// <summary>
///     Interaction logic for RenameViewWindow.xaml
/// </summary>
public partial class RenameViewWindow {
    public RenameViewWindow() {
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitCopyViews);
    public override string ProjectConfigName => nameof(RenameViewWindow);

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }

    private void CloseCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e) {
        DialogResult = false;
    }
}
