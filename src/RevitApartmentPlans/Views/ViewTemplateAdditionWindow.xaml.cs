using System.Windows;

namespace RevitApartmentPlans.Views {
    public partial class ViewTemplateAdditionWindow {
        public ViewTemplateAdditionWindow() {
            InitializeComponent();
        }


        public override string PluginName => nameof(RevitApartmentPlans);
        public override string ProjectConfigName => nameof(ViewTemplateAdditionWindow);

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
