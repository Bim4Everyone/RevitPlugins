using System.Windows;

namespace RevitClashDetective.Views {
    /// <summary>
    /// Interaction logic for RevitReportClashNavigator.xaml
    /// </summary>
    public partial class RevitReportClashNavigator {
        public RevitReportClashNavigator() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitClashDetective);
        public override string ProjectConfigName => nameof(RevitReportClashNavigator);

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
