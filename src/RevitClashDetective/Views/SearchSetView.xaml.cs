namespace RevitClashDetective.Views {
    public partial class SearchSetView {
        public SearchSetView() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitClashDetective);
        public override string ProjectConfigName => nameof(SearchSetView);
    }
}
