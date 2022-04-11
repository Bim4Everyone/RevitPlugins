using System.Windows;

namespace RevitWindowGapPlacement.Views {
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();
        }
        
        public override string PluginName => nameof(RevitWindowGapPlacement);
        public override string ProjectConfigName => nameof(MainWindow);
    }
}