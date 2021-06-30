using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

    public class TabExtensionsSettingsViewModel : ITabSetting, INotifyPropertyChanged {
        public TabExtensionsSettingsViewModel() {
            Name = "Расширения";
            Content = new TabExtensionsSettingsView() { ViewModel = this };

            IEnumerable<PyRevitExtensionViewModel> extensions = GetExtensions();
            PyRevitExtensions = new ObservableCollection<PyRevitExtensionViewModel>(extensions);
            OnPropertyChanged(nameof(PyRevitExtensions));
        }

        public string Name { get; }
        public object Content { get; }
        public ObservableCollection<PyRevitExtensionViewModel> PyRevitExtensions { get; }

        public void SaveSettings() {
            foreach(var extension in PyRevitExtensions) {
                extension.ToggleExtension.Toggle(extension.Enabled);
            }
        }

        private static IEnumerable<PyRevitExtensionViewModel> GetExtensions() {
            var bimExtension = new BimExtensions();
            var pyExtension = new PyExtensions();

            return bimExtension.GetPyRevitExtensionViewModels().Union(pyExtension.GetPyRevitExtensionViewModels());
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
