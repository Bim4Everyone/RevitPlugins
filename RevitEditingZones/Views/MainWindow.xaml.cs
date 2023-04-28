using System.Windows;

namespace RevitEditingZones.Views {
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitEditingZones);
        public override string ProjectConfigName => nameof(MainWindow);

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}