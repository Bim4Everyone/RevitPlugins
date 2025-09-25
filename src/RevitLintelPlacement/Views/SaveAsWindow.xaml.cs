using System.Windows;

namespace RevitLintelPlacement.Views;

/// <summary>
///     Interaction logic for SaveAsWindow.xaml
/// </summary>
public partial class SaveAsWindow : Window {
    public SaveAsWindow() {
        InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e) {
        Close();
    }
}
