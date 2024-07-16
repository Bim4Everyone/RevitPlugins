using dosymep.Bim4Everyone.SimpleServices;

namespace RevitPlatformSettings.ViewModels.Settings {
    internal sealed class AboutSettingsViewModel : SettingsViewModel {
        private readonly IPlatformSettingsService _platformSettingsService;

        public AboutSettingsViewModel(
            int id, int parentId, string settingsName,
            IPlatformSettingsService platformSettingsService)
            : base(id, parentId, settingsName) {
            _platformSettingsService = platformSettingsService;
        }
    }
}
