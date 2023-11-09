using System.Windows;

namespace RevitOpeningPlacement.Views {
    /// <summary>
    /// Interaction logic for ReportView.xaml
    /// </summary>
    public partial class ReportView {
        public ReportView() {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        public override string PluginName => nameof(RevitOpeningPlacement);
        public override string ProjectConfigName => nameof(ReportView);

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            _dg.GroupBy(_dg.Columns[1]);
            var row = _dg.GetRow(0);
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
