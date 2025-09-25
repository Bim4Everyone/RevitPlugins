using System.Windows;

namespace RevitLintelPlacement.Views;

/// <summary>
///     Interaction logic for LintelsConfigView.xaml
/// </summary>
public partial class LintelsConfigView {
    public LintelsConfigView() {
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitLintelPlacement);
    public override string ProjectConfigName => nameof(LintelsConfigView);

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }

    private void SimpleButtonOK_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }
}
