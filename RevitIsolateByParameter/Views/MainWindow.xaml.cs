using System.Windows;

namespace RevitIsolateByParameter.Views {
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitIsolateByParameter);
        public override string ProjectConfigName => nameof(MainWindow);

    }
}