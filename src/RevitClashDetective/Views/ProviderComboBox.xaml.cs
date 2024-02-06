using System.Windows.Controls;

namespace RevitClashDetective.Views {
    /// <summary>
    /// Interaction logic for ProviderCombobBox.xaml
    /// </summary>
    public partial class ProviderComboBox : UserControl {

        public ProviderComboBox() {
            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ComboBox comboBox = (ComboBox) sender;
            comboBox.SelectedItem = null;
        }
    }
}
