using System.Windows;

namespace RevitMirroredElements.Views {

    public partial class CategoriesWindow : Window {
        public CategoriesWindow() {
            InitializeComponent();
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
