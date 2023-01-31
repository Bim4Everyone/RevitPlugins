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
using System.Windows.Shapes;

namespace RevitCheckingLevels.Views {
    /// <summary>
    /// Interaction logic for CheckingLinkLevelsWindow.xaml
    /// </summary>
    public partial class CheckingLinkLevelsWindow {
        public CheckingLinkLevelsWindow() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitCheckingLevels);
        public override string ProjectConfigName => nameof(CheckingLinkLevelsWindow);

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
