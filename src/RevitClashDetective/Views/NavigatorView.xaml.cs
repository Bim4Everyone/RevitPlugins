using System.Windows;

using DevExpress.Xpf.Grid;

using RevitClashDetective.ViewModels.Navigator;

namespace RevitClashDetective.Views {
    /// <summary>
    /// Interaction logic for NavigatorView.xaml
    /// </summary>
    public partial class NavigatorView {
        public NavigatorView() {
            InitializeComponent();
            var column1 = new GridColumn() { FieldName = $"{nameof(IClashViewModel.FirstElementFields)}.Azaza", Header = "Test1", VisibleIndex = 8 };
            var column2 = new GridColumn() { FieldName = $"{nameof(IClashViewModel.SecondElementFields)}.Azaza", Header = "Test2", VisibleIndex = 15 };
            _dg.Columns.Add(column1);
            _dg.Columns.Add(column2);
        }

        public override string PluginName => nameof(RevitClashDetective);
        public override string ProjectConfigName => nameof(NavigatorView);

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
