using System.Windows;

namespace RevitGenLookupTables.Views;

/// <summary>
///     Interaction logic for LookupTablesWindow.xaml
/// </summary>
public partial class LookupTablesWindow {
    public LookupTablesWindow() {
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitGenLookupTables);
    public override string ProjectConfigName => nameof(LookupTablesWindow);

    private void ButtonOK_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
