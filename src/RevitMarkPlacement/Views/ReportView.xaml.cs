using System.Windows;

using dosymep.WPF.Views;

namespace RevitMarkPlacement.Views;

/// <summary>
///     Interaction logic for ReportView.xaml
/// </summary>
public partial class ReportView : PlatformWindow {
    public ReportView() {
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitMarkPlacement);
    public override string ProjectConfigName => nameof(ReportView);

    private void Button_Click(object sender, RoutedEventArgs e) {
        Close();
    }
}
