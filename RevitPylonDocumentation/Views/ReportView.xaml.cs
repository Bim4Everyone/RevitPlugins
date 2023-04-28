using System.Windows;

namespace RevitPylonDocumentation.Views {
    /// <summary>
    /// Логика взаимодействия для ReportView.xaml
    /// </summary>
    public partial class ReportView {
        public ReportView() {
            InitializeComponent();
        }
        public override string PluginName => nameof(RevitPylonDocumentation);
        public override string ProjectConfigName => nameof(MainWindow);
    }
}
