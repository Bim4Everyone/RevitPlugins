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

using dosymep.WPF.Views;

namespace RevitLintelPlacement.Views {
    /// <summary>
    /// Interaction logic for LintelsConfigView.xaml
    /// </summary>
    public partial class LintelsConfigView {
        public LintelsConfigView() {
            InitializeComponent();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        public override string PluginName => nameof(RevitLintelPlacement);
        public override string ProjectConfigName => nameof(LintelsConfigView);

        private void SimpleButtonOK_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }
    }
}
