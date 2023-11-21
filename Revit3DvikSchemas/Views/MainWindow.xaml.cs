using System.Windows;

namespace Revit3DvikSchemas.Views {
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();
        }

        public override string PluginName => nameof(Revit3DvikSchemas);
        public override string ProjectConfigName => nameof(MainWindow);

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }



        private void ListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {

        }
    }
}
