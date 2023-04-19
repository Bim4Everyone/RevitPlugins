using PlatformSettings.ViewModels;

namespace PlatformSettings.Factories {
    internal interface ISettingsViewModelFactory {
        T Create<T>(int id, int parentId, string settingsName) where T : SettingsViewModel;
    }
}