using System.Windows;
using System.Windows.Input;

namespace RevitOverridingGraphicsInViews.Views {
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitOverridingGraphicsInViews);
        public override string ProjectConfigName => nameof(MainWindow);

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {

            if(e.ChangedButton == MouseButton.Left) {
                this.DragMove();
            }
        }

        private void CloseCommandBinding_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e) {

            this.Close();
        }
    }
}