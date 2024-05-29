namespace RevitArchitecturalDocumentation.Views {
    /// <summary>
    /// Логика взаимодействия для TreeReportV.xaml
    /// </summary>
    public partial class TreeReportV {
        public TreeReportV() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitArchitecturalDocumentation);
        public override string ProjectConfigName => nameof(TreeReportV);
    }
}
