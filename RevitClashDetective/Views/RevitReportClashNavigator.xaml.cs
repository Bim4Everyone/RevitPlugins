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

namespace RevitClashDetective.Views {
    /// <summary>
    /// Interaction logic for RevitReportClashNavigator.xaml
    /// </summary>
    public partial class RevitReportClashNavigator {
        public RevitReportClashNavigator() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitClashDetective);
        public override string ProjectConfigName => nameof(RevitReportClashNavigator);

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
