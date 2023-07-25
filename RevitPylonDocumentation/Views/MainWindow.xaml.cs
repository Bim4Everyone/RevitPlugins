using System.Windows;
using System.Windows.Controls;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Views {
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitPylonDocumentation);
        public override string ProjectConfigName => nameof(MainWindow);

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) 
        {
            DialogResult = false;
        }

        private void SelectAllHostMarks(object sender, RoutedEventArgs e) {
            hostMarks.SelectAll();
        }
        private void UnselectAllHostMarks(object sender, RoutedEventArgs e) {
            hostMarks.UnselectAll();
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }
    }
}