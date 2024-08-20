using System.Windows;

namespace RevitClashDetective.Views {
    /// <summary>
    /// Interaction logic for NavigatorView.xaml
    /// </summary>
    public partial class NavigatorView {
        public NavigatorView() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitClashDetective);
        public override string ProjectConfigName => nameof(NavigatorView);

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
