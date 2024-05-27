namespace RevitFinishingWalls.Views {
    public partial class ErrorWindow {
        public ErrorWindow() {
            InitializeComponent();
        }


        public override string PluginName => nameof(RevitFinishingWalls);
        public override string ProjectConfigName => nameof(MainWindow);
    }
}
