using System.Windows;

using DevExpress.Xpf.Grid;

namespace RevitOpeningPlacement.Views {
    /// <summary>
    /// Класс окна для отображения исходящих заданий на отверстия в файле инженерных систем
    /// </summary>
    public partial class NavigatorMepOutcomingView {
        /// <summary>
        /// Конструктор окна для отображения исходящих заданий на отверстия в файле инженерных систем
        /// </summary>
        public NavigatorMepOutcomingView() {
            InitializeComponent();
            Loaded += NavigatorView_Loaded;
        }

        private void NavigatorView_Loaded(object sender, RoutedEventArgs e) {
            _dg.GroupBy(_dg.Columns[1]);
            _dg.GroupBy(_dg.Columns[2]);
        }

        public override string PluginName => nameof(RevitOpeningPlacement);
        public override string ProjectConfigName => nameof(NavigatorMepOutcomingView);

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
