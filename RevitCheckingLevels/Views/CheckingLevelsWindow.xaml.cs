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

using DevExpress.Utils.CommonDialogs.Internal;

namespace RevitCheckingLevels.Views {
    /// <summary>
    /// Interaction logic for CheckingLevelsWindow.xaml
    /// </summary>
    public partial class CheckingLevelsWindow {
        public CheckingLevelsWindow() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitCheckingLevels);
        public override string ProjectConfigName => nameof(CheckingLevelsWindow);

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
