using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RevitKrChecker.Views {
    /// <summary>
    /// Логика взаимодействия для ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow {

        public ReportWindow() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitKrChecker);
        public override string ProjectConfigName => nameof(ReportWindow);

        private void Button_Click_ListViewExpander_Expand(object sender, RoutedEventArgs e) {
            foreach(GroupItem gi in FindVisualChildren<GroupItem>(reportList))
                gi.Tag = true;
        }

        private void Button_Click_ListViewExpander_Collapse(object sender, RoutedEventArgs e) {
            foreach(GroupItem gi in FindVisualChildren<GroupItem>(reportList))
                gi.Tag = false;
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject {
            if(depObj != null) {
                for(int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++) {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if(child != null && child is T) {
                        yield return (T) child;
                    }

                    foreach(T childOfChild in FindVisualChildren<T>(child)) {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}
