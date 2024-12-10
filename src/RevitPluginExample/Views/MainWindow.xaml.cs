using System.Windows;

using Wpf.Ui.Appearance;

namespace RevitPluginExample.Views {
    public partial class MainWindow : BaseWindow {
        public MainWindow() {
            InitializeComponent();
            ApplicationThemeManager.Apply(this);
        }

        public override string PluginName => nameof(RevitPluginExample);
        public override string ProjectConfigName => nameof(MainWindow);

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void ButtonChangeTheme_Click(object sender, RoutedEventArgs e) {
            ChangeTheme();
        }

    }
}
