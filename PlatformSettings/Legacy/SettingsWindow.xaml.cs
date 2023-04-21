using System.Collections.ObjectModel;
using System.Windows;

using dosymep.WPF.Views;

using PlatformSettings.Legacy.SharedParams;
using PlatformSettings.Legacy.TabExtensions;

namespace PlatformSettings.Legacy {
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : PlatformWindow {
        public SettingsWindow() {
            InitializeComponent();
        }

        public override string PluginName => nameof(PlatformSettings);
        public override string ProjectConfigName => nameof(SettingsWindow);

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
            SharedParamsSettingsViewModel = new RevitParamsSettingsViewModel(new SharedParams.SharedParams() { Name = "Общие параметры", KeyName = "SharedParamsPath", ConfigFileName = "shared_params.json" });
            ProjectParamsSettingsViewModel = new RevitParamsSettingsViewModel(new SharedParams.ProjectParams() { Name = "Параметры проекта", KeyName = "ProjectParamsPath", ConfigFileName = "project_params.json" });

            TabSettings = new ObservableCollection<ITabSetting> {
                TabExtensionsSettingsViewModel,
                SharedParamsSettingsViewModel,
                ProjectParamsSettingsViewModel
            };
        }

        public ObservableCollection<ITabSetting> TabSettings { get; }
        public TabExtensionsSettingsViewModel TabExtensionsSettingsViewModel { get; set; }
        public RevitParamsSettingsViewModel SharedParamsSettingsViewModel { get; set; }
        public RevitParamsSettingsViewModel ProjectParamsSettingsViewModel { get; set; }

        public void SaveSettings() {
            TabExtensionsSettingsViewModel?.SaveSettings();
            SharedParamsSettingsViewModel.SaveSettings();
            ProjectParamsSettingsViewModel.SaveSettings();
        }
    }

    public interface ITabSetting {
        string Name { get; }
        object Content { get; }
    }
}
