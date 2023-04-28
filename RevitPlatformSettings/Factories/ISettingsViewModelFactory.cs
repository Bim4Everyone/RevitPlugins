using RevitPlatformSettings.ViewModels;

namespace RevitPlatformSettings.Factories {
    internal interface ISettingsViewModelFactory {
        T Create<T>(int id, int parentId, string settingsName) where T : SettingsViewModel;
    }
}