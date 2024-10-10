using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace RevitDeclarations.Views {
    public partial class ApartmentMainWindow {
        public ApartmentMainWindow() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitDeclarations);
        public override string ProjectConfigName => nameof(ApartmentMainWindow);
        
        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
