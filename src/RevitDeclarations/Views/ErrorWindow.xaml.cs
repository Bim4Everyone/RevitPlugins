using System.Windows;

namespace RevitDeclarations.Views;
public partial class ErrorWindow : Window {
    public ErrorWindow() {
        InitializeComponent();
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
