using System.Windows;

namespace RevitClashDetective.Views {
    /// <summary>
    /// Interaction logic for FilterNameView.xaml
    /// </summary>
    public partial class FilterNameView {
        public FilterNameView() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitClashDetective);
        public override string ProjectConfigName => nameof(FilterNameView);

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
