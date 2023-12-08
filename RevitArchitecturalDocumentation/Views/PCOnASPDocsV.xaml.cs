using System.Windows;

namespace RevitArchitecturalDocumentation.Views {
    public partial class PCOnASPDocsV {
        public PCOnASPDocsV() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitArchitecturalDocumentation);
        public override string ProjectConfigName => nameof(PCOnASPDocsV);

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }
    }
}