using System.Windows;

namespace RevitCopingZones.Views {
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitCopingZones);
        public override string ProjectConfigName => nameof(MainWindow);

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }
    }
}