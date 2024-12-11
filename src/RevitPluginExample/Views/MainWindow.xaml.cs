using System.Windows;

namespace RevitPluginExample.Views {
    public partial class MainWindow : BaseWindow {
        public MainWindow() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitPluginExample);
        public override string ProjectConfigName => nameof(MainWindow);

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
