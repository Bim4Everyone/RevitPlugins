using System.Windows;
using System.Windows.Controls;

using RevitClashDetective.ViewModels.FilterCreatorViewModels;

namespace RevitClashDetective.Views {
    /// <summary>
    /// Interaction logic for CategoryView.xaml
    /// </summary>
    public partial class CategoryView : UserControl {
        public static DependencyProperty IsMassSelectionProperty = DependencyProperty.Register(nameof(IsMassSelection), typeof(bool), typeof(CategoryView));

        public bool IsMassSelection {
            get => (bool) GetValue(IsMassSelectionProperty);
            set => SetValue(IsMassSelectionProperty, value);
        }

        public CategoryView() {
            InitializeComponent();
        }

        private void _che_Checked(object sender, RoutedEventArgs e) {
            _dg.RefreshData();
        }

        private void _dg_CustomRowFilter(object sender, DevExpress.Xpf.Grid.RowFilterEventArgs e) {
            if(!(_dg.GetRow(e.ListSourceRowIndex) as CategoryViewModel)?.IsSelected == true)
                e.Visible = !_che.IsChecked.Value;
            e.Handled = !e.Visible ? true : false;
        }
    }
}
