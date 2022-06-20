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

using DevExpress.Xpf.Core;

namespace RevitSuperfilter.Views {
    /// <summary>
    /// Interaction logic for ParamsView.xaml
    /// </summary>
    public partial class ParamsView : UserControl {
        private bool _isChecked;

        public ParamsView() {
            InitializeComponent();
        }

        private void SimpleButton_Click(object sender, RoutedEventArgs e) {
            if(!_isChecked) {
                _treeView.CheckAllNodes();
            } else {
                _treeView.UncheckAllNodes();
            }

            _isChecked = _treeView.Nodes
                .SelectMany(item => item.Nodes)
                .Where(item => item.IsVisible)
                .Any(item => item.IsChecked == true);
            _sb.Content = _isChecked ? "Cнять выделение" : "Выделить всё";
        }

        private void _treeView_FilterChanged(object sender, RoutedEventArgs e) {
            _isChecked = _treeView.Nodes
                .SelectMany(item => item.Nodes)
                .Where(item=>item.IsVisible)
                .Any(item => item.IsChecked == true);
            _sb.Content = _isChecked ? "Cнять выделение" : "Выделить всё";
        }
    }
}
