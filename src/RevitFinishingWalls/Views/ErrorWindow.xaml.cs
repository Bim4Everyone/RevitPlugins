namespace RevitFinishingWalls.Views {
    public partial class ErrorWindow {
        public ErrorWindow() {
            InitializeComponent();
            Loaded += ThemedPlatformWindow_Loaded;
        }


        public override string PluginName => nameof(RevitFinishingWalls);
        public override string ProjectConfigName => nameof(ErrorWindow);

        private void ThemedPlatformWindow_Loaded(object sender, System.Windows.RoutedEventArgs e) {
            //_dgErrors.GroupBy(nameof(ErrorViewModel.Message));
        }
    }
}
