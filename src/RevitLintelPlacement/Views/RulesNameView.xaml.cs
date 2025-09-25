using System.Windows;

namespace RevitLintelPlacement.Views;

/// <summary>
///     Interaction logic for RulesNameView.xaml
/// </summary>
public partial class RulesNameView {
    public RulesNameView() {
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitLintelPlacement);
    public override string ProjectConfigName => nameof(RulesNameView);

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }
}
