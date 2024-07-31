using System.Windows;

namespace RevitOpeningPlacement.Views {
    public partial class MepCategoryFilterView {
        public MepCategoryFilterView() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitOpeningPlacement);

        public override string ProjectConfigName => nameof(MepCategoryFilterView);

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
