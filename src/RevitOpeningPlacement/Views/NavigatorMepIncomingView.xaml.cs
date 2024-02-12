using System.Windows;

using DevExpress.Xpf.Grid;

namespace RevitOpeningPlacement.Views {
    /// <summary>
    /// Класс окна для отображения входящих заданий на отверстия в файле архитектора или конструктора
    /// </summary>
    public partial class NavigatorMepIncomingView {
        /// <summary>
        /// Конструктор окна для отображения входящих заданий на отверстия в файле архитектора или конструктора
        /// </summary>
        public NavigatorMepIncomingView() {
            InitializeComponent();
            Loaded += NavigatorView_Loaded;
        }

        private void NavigatorView_Loaded(object sender, RoutedEventArgs e) {
            _dgIncomingTasks.GroupBy(_dgIncomingTasks.Columns[1]);
            _dgIncomingTasks.GroupBy(_dgIncomingTasks.Columns[2]);
            _dgIncomingTasks.GroupBy(_dgIncomingTasks.Columns[3]);

            _dgIncomingTasks.SortBy(_dgIncomingTasks.Columns[13], DevExpress.Data.ColumnSortOrder.Descending);
            _dgIncomingTasks.SortBy(_dgIncomingTasks.Columns[11]);

            _dgOpeningsReal.GroupBy(_dgOpeningsReal.Columns[1]);
        }

        public override string PluginName => nameof(RevitOpeningPlacement);
        public override string ProjectConfigName => nameof(NavigatorMepIncomingView);

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
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

        private void SimpleButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
