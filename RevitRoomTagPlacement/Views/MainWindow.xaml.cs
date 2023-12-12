using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using RevitRoomTagPlacement.ViewModels;

namespace RevitRoomTagPlacement.Views {
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitRoomTagPlacement);
        public override string ProjectConfigName => nameof(MainWindow);
        
        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void IndentValidation(object sender, TextCompositionEventArgs e) {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if(e.Key == Key.Space)
                e.Handled = true;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e) {
            ChangeSelected(true);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e) {
            ChangeSelected(false);
        }

        private void ChangeSelected(bool state) {
            var listBox = (ListBox) FindName("RoomGroups");
            var groups = listBox.SelectedItems;
            foreach(RoomGroupViewModel group in groups) {
                group.IsChecked = state;
            }
        }
    }
}
