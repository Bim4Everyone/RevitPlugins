using System.Windows;

using dosymep.SimpleServices;

namespace RevitRoomExtrusion.Views {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        public string PluginName => nameof(RevitRoomExtrusion);
        public string ProjectConfigName => nameof(MainWindow);        

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
