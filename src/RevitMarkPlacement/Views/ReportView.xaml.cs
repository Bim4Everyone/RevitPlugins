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

namespace RevitMarkPlacement.Views {
    /// <summary>
    /// Interaction logic for ReportView.xaml
    /// </summary>
    public partial class ReportView : PlatformWindow {
        public ReportView() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitMarkPlacement);
        public override string ProjectConfigName => nameof(ReportView);

        private void Button_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
