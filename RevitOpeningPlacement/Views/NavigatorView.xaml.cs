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

using DevExpress.Xpf.Grid;

using RevitClashDetective.Models;

using RevitOpeningPlacement.ViewModels.Navigator;

namespace RevitOpeningPlacement.Views {
    /// <summary>
    /// Interaction logic for NavigatorView.xaml
    /// </summary>
    public partial class NavigatorView {

        public NavigatorView() {
            InitializeComponent();
            Loaded += NavigatorView_Loaded;
        }

        private void NavigatorView_Loaded(object sender, RoutedEventArgs e) {
            _dg.GroupBy(_dg.Columns[1]);
        }

        public override string PluginName => nameof(RevitOpeningPlacement);
        public override string ProjectConfigName => nameof(NavigatorView);

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void view_FocusedRowHandleChanged(object sender, FocusedRowHandleChangedEventArgs e) {
            var handle = _dg.View.FocusedRowHandle;
            _dg.UnselectAll();
            _dg.SelectItem(handle);
        }

        private void SimpleButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
