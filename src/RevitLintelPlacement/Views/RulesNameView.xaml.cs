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

namespace RevitLintelPlacement.Views {
    /// <summary>
    /// Interaction logic for RulesNameView.xaml
    /// </summary>
    public partial class RulesNameView {
        public RulesNameView() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitLintelPlacement);
        public override string ProjectConfigName => nameof(RulesNameView);

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }
    }
}
