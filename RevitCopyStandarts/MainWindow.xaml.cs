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

using RevitCopyStandarts.ViewModels;

namespace RevitCopyStandarts {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow  {
        public MainWindow() {
            InitializeComponent();
        }


        public override string PluginName => nameof(RevitCopyStandarts);
        public override string ProjectConfigName => nameof(MainWindow);
    }
}
