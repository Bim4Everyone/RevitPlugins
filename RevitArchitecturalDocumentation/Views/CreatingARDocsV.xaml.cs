using System;
using System.Windows;
using System.Windows.Controls;

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

        private void WB_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e) {
            WebBrowser wb = (WebBrowser) sender;

            string script = "document.body.style.overflow ='hidden'";
            wb.InvokeScript("execScript", new Object[] { script, "JavaScript" });
            script = "document.body.style.zoom = '30%';";
            wb.InvokeScript("execScript", new Object[] { script, "JavaScript" });
        }
    }
}
