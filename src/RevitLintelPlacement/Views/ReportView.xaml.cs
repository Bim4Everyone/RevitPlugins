namespace RevitLintelPlacement.Views;

/// <summary>
///     Interaction logic for ReportView.xaml
/// </summary>
public partial class ReportView {
    public ReportView() {
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitLintelPlacement);
    public override string ProjectConfigName => nameof(ReportView);
}
