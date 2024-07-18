using dosymep.Bim4Everyone.SimpleServices;

namespace RevitPlatformSettings.ViewModels.Settings {
    internal sealed class AboutSettingsViewModel : SettingsViewModel {
        private readonly IPlatformSettingsService _platformSettingsService;

        public AboutSettingsViewModel(
            int id, int parentId, string settingsName,
            IPlatformSettingsService platformSettingsService)
            : base(id, parentId, settingsName) {
            _platformSettingsService = platformSettingsService;

            ImagePath = _platformSettingsService.CorpSettings?.ImagePath 
                        ?? "/dosymep.Bim4Everyone;component/assets/Bim4Everyone.png";
            
            PlatformName = _platformSettingsService.CorpSettings?.Name;
            PlatformPageUrl = _platformSettingsService.SocialsSettings?.PlatformPageUrl;
            
            NewsChatUrl = _platformSettingsService.SocialsSettings?.NewsChatUrl;
            MainChatUrl = _platformSettingsService.SocialsSettings?.MainChatUrl;
            DownloadLinkUrl = _platformSettingsService.SocialsSettings?.DownloadLinkUrl;
        }

        public string ImagePath { get; }
        public string PlatformName { get; }
        public string PlatformPageUrl { get; }
        
        public string NewsChatUrl { get; }
        public string MainChatUrl { get; }
        public string DownloadLinkUrl { get; }
    }
}
