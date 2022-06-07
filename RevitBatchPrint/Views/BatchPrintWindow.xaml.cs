using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using dosymep.WPF.Views;

using RevitBatchPrint.Models;

namespace RevitBatchPrint.Views {
    /// <summary>
    /// Interaction logic for PrintViewSheetNames.xaml
    /// </summary>
    public partial class BatchPrintWindow {
        public BatchPrintWindow() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitBatchPrint);
        public override string ProjectConfigName => nameof(BatchPrintWindow);

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ComboBox comboBox = (ComboBox) sender;
            comboBox.SelectedItem = null;
        }
    }
}
