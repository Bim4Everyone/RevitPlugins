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
using System.Windows.Shapes;

using RevitFinishing.ViewModels;

namespace RevitFinishing.Views
{
    /// <summary>
    /// Interaction logic for ErrorsWindow.xaml
    /// </summary>
    public partial class ErrorsWindow : Window
    {
        internal ErrorsWindow(ErrorsViewModel errorsViewModel)
        {
            DataContext = errorsViewModel;
            InitializeComponent();
        }
    }
}
