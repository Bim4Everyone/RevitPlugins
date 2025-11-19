using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RevitPylonDocumentation.Views.Pages;
/// <summary>
/// Логика взаимодействия для GeneralPage.xaml
/// </summary>
public partial class GeneralPage : Page {
    public GeneralPage() {
        InitializeComponent();
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        var window = Window.GetWindow(this) as MainWindow;
        if(window != null) {
            window.DialogResult = true;
        }
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        var window = Window.GetWindow(this) as MainWindow;
        if(window != null) {
            window.DialogResult = false;
        }
    }
}
