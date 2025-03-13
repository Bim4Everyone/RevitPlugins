using RevitPlatformSettings.ViewModels;
using RevitPlatformSettings.ViewModels.Settings;

namespace RevitPlatformSettings.Factories {
    internal interface ISettingsViewModelFactory {
        T Create<T>(string settingsName) where T : SettingsViewModel;
    }
}
