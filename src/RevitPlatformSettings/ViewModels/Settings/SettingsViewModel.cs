using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;

namespace RevitPlatformSettings.ViewModels.Settings {
    internal class SettingsViewModel : BaseViewModel {
        public SettingsViewModel(string settingsName) {
            SettingsName = settingsName;
        }
        public string SettingsName { get; }
        public ObservableCollection<SettingsViewModel> Settings { get; set; }

        public virtual void SaveSettings() { }
    }
}
