using System.Windows;


namespace RevitArchitecturalDocumentation.Views {
    public partial class CopySpecSheetInstanceV {
        public CopySpecSheetInstanceV() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitArchitecturalDocumentation);
        public override string ProjectConfigName => nameof(CopySpecSheetInstanceV);

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
