using System.Windows;

namespace RevitArchitecturalDocumentation.Views {
    public partial class PCOnASPDocsView {
        public PCOnASPDocsView() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitArchitecturalDocumentation);
        public override string ProjectConfigName => nameof(PCOnASPDocsView);

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}