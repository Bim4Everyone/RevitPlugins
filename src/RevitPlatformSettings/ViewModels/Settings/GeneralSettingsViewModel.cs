using dosymep.Bim4Everyone.SimpleServices;

namespace RevitPlatformSettings.ViewModels.Settings {
    internal sealed class GeneralSettingsViewModel : SettingsViewModel {
        private readonly IPlatformSettingsService _platformSettingsService;

        public GeneralSettingsViewModel(
            int id, int parentId, string settingsName,
            IPlatformSettingsService platformSettingsService)
            : base(id, parentId, settingsName) {
            _platformSettingsService = platformSettingsService;
        }
    }
}
