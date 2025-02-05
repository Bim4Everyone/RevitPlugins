using System.Windows;

namespace RevitRefreshLinks.Views {
    public partial class RsOpenFileWindow {
        public RsOpenFileWindow() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitRefreshLinks);
        public override string ProjectConfigName => nameof(RsOpenFileWindow);

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
