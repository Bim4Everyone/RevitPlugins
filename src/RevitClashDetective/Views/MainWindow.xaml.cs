using System.Windows;

using DevExpress.Xpf.Grid;

namespace RevitClashDetective.Views {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitClashDetective);
        public override string ProjectConfigName => nameof(MainWindow);

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void _gridView_CellValueChanging(object sender, CellValueChangedEventArgs e) {
            (sender as TableView).PostEditor();
        }
    }
}
