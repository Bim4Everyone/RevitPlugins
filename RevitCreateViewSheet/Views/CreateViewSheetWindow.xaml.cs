using System;
using System.Collections.Generic;
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

namespace RevitCreateViewSheet.Views {
    /// <summary>
    /// Interaction logic for CreateViewSheetWindow.xaml
    /// </summary>
    public partial class CreateViewSheetWindow {
        public CreateViewSheetWindow() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitCreateViewSheet);
        public override string ProjectConfigName => nameof(CreateViewSheetWindow);

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
