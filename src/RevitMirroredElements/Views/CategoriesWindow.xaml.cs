using System.Windows;

namespace RevitMirroredElements.Views {

    public partial class CategoriesWindow : Window {
        public CategoriesWindow() {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }
    }
}
