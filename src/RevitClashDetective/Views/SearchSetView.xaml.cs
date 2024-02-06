using System.Windows;

namespace RevitClashDetective.Views {
    /// <summary>
    /// Interaction logic for SearchSetView.xaml
    /// </summary>
    public partial class SearchSetView {
        public SearchSetView() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitClashDetective);
        public override string ProjectConfigName => nameof(SearchSetView);

        private void GridControl_Loaded(object sender, RoutedEventArgs e) {
            _gridView.BestFitColumns();
        }
    }
}
