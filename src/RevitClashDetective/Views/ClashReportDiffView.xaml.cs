using System.Windows;

namespace RevitClashDetective.Views {
    /// <summary>
    /// Interaction logic for ClashReportDiffView.xaml
    /// </summary>
    public partial class ClashReportDiffView {
        public ClashReportDiffView() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitClashDetective);
        public override string ProjectConfigName => nameof(ClashReportDiffView);

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
