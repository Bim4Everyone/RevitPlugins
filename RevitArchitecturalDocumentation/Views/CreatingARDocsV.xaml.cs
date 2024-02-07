using System.Windows;

namespace RevitArchitecturalDocumentation.Views {
    public partial class CreatingARDocsV {
        public CreatingARDocsV() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitArchitecturalDocumentation);
        public override string ProjectConfigName => nameof(CreatingARDocsV);

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }
    }
}
