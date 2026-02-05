using System.Windows;

namespace RevitOpeningPlacement.Views.Utils;
public partial class ReportView {
    public ReportView() {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    public override string PluginName => nameof(RevitOpeningPlacement);
    public override string ProjectConfigName => nameof(ReportView);

    private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
        _dg.GroupBy(_dg.Columns[1]);
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        Close();
    }
}
