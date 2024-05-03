using System.Windows;

namespace RevitOpeningPlacement.Views {
    /// <summary>
    /// Interaction logic for MepCategoryFilterView.xaml
    /// </summary>
    public partial class MepCategoryFilterView {
        public MepCategoryFilterView() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitOpeningPlacement);

        public override string ProjectConfigName => nameof(MepCategoryFilterView);

        private void GridControl_Loaded(object sender, RoutedEventArgs eventArgs) {
            _gridLinearView.BestFitColumns();
            _gridNonLinearView.BestFitColumns();
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
