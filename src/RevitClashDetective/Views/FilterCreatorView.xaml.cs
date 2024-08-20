using System.Windows;
using System.Windows.Controls;

namespace RevitClashDetective.Views {
    /// <summary>
    /// Interaction logic for FilterCreatorView.xaml
    /// </summary>
    public partial class FilterCreatorView {
        public FilterCreatorView() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitClashDetective);
        public override string ProjectConfigName => nameof(FilterCreatorView);

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void CriterionControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            ((ContentControl) sender).Content = new CriterionView() { DataContext = ((ContentControl) sender).DataContext };
        }

        private void CategoryControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            ((ContentControl) sender).Content = new CategoryView() { DataContext = ((ContentControl) sender).DataContext };
        }
    }
}
