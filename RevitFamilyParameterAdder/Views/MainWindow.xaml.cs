using System.Windows;

namespace RevitFamilyParameterAdder.Views {
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitFamilyParameterAdder);
        public override string ProjectConfigName => nameof(MainWindow);

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }


        private void SelectAllParams(object sender, RoutedEventArgs e) {
            parameters.SelectAll();
        }
        private void UnselectAllParams(object sender, RoutedEventArgs e) {
            parameters.UnselectAll();
        }
    }
}