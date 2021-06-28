using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace PlatformSettings.TabExtensions {
    /// <summary>
    /// Interaction logic for TabsSettingsView.xaml
    /// </summary>
    public partial class TabExtensionsSettingsView : UserControl {
        public static readonly DependencyProperty PyRevitExtensionsProperty = DependencyProperty.Register(nameof(PyRevitExtensions), typeof(ObservableCollection<PyRevitExtensionViewModel>), typeof(TabExtensionsSettingsView));

        public TabExtensionsSettingsView() {
            InitializeComponent();
        }

        public ObservableCollection<PyRevitExtensionViewModel> PyRevitExtensions {
            get { return (ObservableCollection<PyRevitExtensionViewModel>) GetValue(PyRevitExtensionsProperty); }
            set { SetValue(PyRevitExtensionsProperty, value); }
        }
    }
}
