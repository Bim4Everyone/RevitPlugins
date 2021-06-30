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

using PlatformSettings.TabExtensions;

namespace PlatformSettings {
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window {
        public SettingsWindow() {
            InitializeComponent();
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }

    public class PlatformSettingsViewModel {
        public PlatformSettingsViewModel() {
            TabExtensionsSettingsViewModel = new TabExtensionsSettingsViewModel();

            TabSettings = new ObservableCollection<ITabSetting> {
                TabExtensionsSettingsViewModel
            };
        }

        public ObservableCollection<ITabSetting> TabSettings { get; }
        public TabExtensionsSettingsViewModel TabExtensionsSettingsViewModel { get; set; }

        public void SaveSettings() {
            TabExtensionsSettingsViewModel?.SaveSettings();
        }
    }

    public interface ITabSetting {
        string Name { get; }
        object Content { get; }
    }
}
