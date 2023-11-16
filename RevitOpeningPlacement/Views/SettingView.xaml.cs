using System.Windows.Controls;

using DevExpress.Xpf.Grid;

namespace RevitOpeningPlacement.Views {
    /// <summary>
    /// Interaction logic for SettingView.xaml
    /// </summary>
    public partial class SettingView : UserControl {
        public SettingView() {
            InitializeComponent();
        }

        private void TableView_CellValueChanging(object sender, CellValueChangedEventArgs e) {
            (sender as TableView).PostEditor();
        }
    }
}
