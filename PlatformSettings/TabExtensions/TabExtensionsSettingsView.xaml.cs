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
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(TabExtensionsSettingsViewModel), typeof(TabExtensionsSettingsView));

        public TabExtensionsSettingsView() {
            InitializeComponent();
        }

        public TabExtensionsSettingsViewModel ViewModel {
            get { return (TabExtensionsSettingsViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }

    public class TabExtensionsSettingsViewModel {
        public TabExtensionsSettingsViewModel() {
            var bimExtension = new BimExtensions();
            var pyExtension = new PyExtensions();

            var extensions = bimExtension.GetPyRevitExtensionViewModels().Union(pyExtension.GetPyRevitExtensionViewModels());
            PyRevitExtensions = new ObservableCollection<PyRevitExtensionViewModel>(extensions);
        }

        public ObservableCollection<PyRevitExtensionViewModel> PyRevitExtensions { get; set; }

        public void SaveSettings() {
            foreach(var extension in PyRevitExtensions) {
                extension.ToggleExtension.Toggle(extension.Enabled);
            }
        }
    }
}
