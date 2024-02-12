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

namespace RevitCopyViews.Views {
    /// <summary>
    /// Interaction logic for CopyUserView.xaml
    /// </summary>
    public partial class CopyUserWindow {
        public CopyUserWindow() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitCopyViews);
        public override string ProjectConfigName => nameof(CopyUserWindow);

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
