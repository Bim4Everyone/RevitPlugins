using System.Windows;

using RevitReinforcementCoefficient.ViewModels;

namespace RevitReinforcementCoefficient.Views {
    /// <summary>
    /// Логика взаимодействия для ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow : Window {
        internal ReportWindow(ReportVM reportVM) {
            InitializeComponent();
            DataContext = reportVM;
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }
    }
}
