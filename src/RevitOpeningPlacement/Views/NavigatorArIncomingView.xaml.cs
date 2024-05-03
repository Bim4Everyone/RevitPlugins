using System.Windows;

using DevExpress.Xpf.Grid;

namespace RevitOpeningPlacement.Views {
    /// <summary>
    /// Класс окна навигатора по входящим заданиям на отверстия от архитектора в файле конструктора
    /// </summary>
    public partial class NavigatorArIncomingView {
        public NavigatorArIncomingView() {
            InitializeComponent();
            Loaded += NavigatorView_Loaded;
        }

        public override string PluginName => nameof(RevitOpeningPlacement);
        public override string ProjectConfigName => nameof(NavigatorArIncomingView);


        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void SimpleButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void viewTasks_FocusedRowHandleChanged(object sender, FocusedRowHandleChangedEventArgs e) {
            var handle = _dgIncomingTasks.View.FocusedRowHandle;
            _dgIncomingTasks.UnselectAll();
            _dgIncomingTasks.SelectItem(handle);
        }

        private void viewOpenings_FocusedRowHandleChanged(object sender, FocusedRowHandleChangedEventArgs e) {
            var handle = _dgOpeningsReal.View.FocusedRowHandle;
            _dgOpeningsReal.UnselectAll();
            _dgOpeningsReal.SelectItem(handle);
        }

        private void NavigatorView_Loaded(object sender, RoutedEventArgs e) {
            _dgIncomingTasks.GroupBy(_dgIncomingTasks.Columns[1]);
            _dgIncomingTasks.GroupBy(_dgIncomingTasks.Columns[2]);

            _dgIncomingTasks.SortBy(_dgIncomingTasks.Columns[7], DevExpress.Data.ColumnSortOrder.Descending);
            _dgIncomingTasks.SortBy(_dgIncomingTasks.Columns[6]);

            _dgOpeningsReal.GroupBy(_dgOpeningsReal.Columns[1]);
            _dgOpeningsReal.SortBy(_dgOpeningsReal.Columns[6], DevExpress.Data.ColumnSortOrder.Descending);
        }
    }
}
