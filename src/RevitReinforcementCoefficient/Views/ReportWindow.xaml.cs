using System.Windows;

namespace RevitReinforcementCoefficient.Views {
    public partial class ReportWindow {
        public ReportWindow() {
            InitializeComponent();
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonTestForHide_Click(object sender, RoutedEventArgs e) {
            Hide();
        }
    }
}
