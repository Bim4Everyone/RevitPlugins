using System.Windows;

namespace RevitEditingZones.Views {
    public partial class LevelsWindow {
        public LevelsWindow() {
            InitializeComponent();
        }
        
        public override string PluginName => nameof(RevitEditingZones);
        public override string ProjectConfigName => nameof(LevelsWindow);
        
        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}