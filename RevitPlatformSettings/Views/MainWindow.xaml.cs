using System.Windows;

namespace RevitPlatformSettings.Views {
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();
        }
        
        public override string PluginName => nameof(RevitPlatformSettings);
        public override string ProjectConfigName => nameof(MainWindow);
        
        private void ButtonOk_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}