using DevExpress.Mvvm.UI;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Bim4Everyone.SimpleServices.Configuration;

namespace RevitPlatformSettings.ViewModels.Settings {
    internal sealed class GeneralSettingsViewModel : SettingsViewModel {
        private readonly IPlatformSettingsService _platformSettingsService;
        private bool? _notificationIsActive;
        private NotificationScreen? _notificationScreen;
        private NotificationPosition? _notificationPosition;
        private int? _notificationVisibleMaxCount;

        public GeneralSettingsViewModel(
            int id, int parentId, string settingsName,
            IPlatformSettingsService platformSettingsService)
            : base(id, parentId, settingsName) {
            _platformSettingsService = platformSettingsService;

            SettingsPath = _platformSettingsService.CorpSettings.SettingsPath;

            NotificationIsActive = _platformSettingsService.NotificationSettings.IsActive;
            NotificationScreen = _platformSettingsService.NotificationSettings.NotificationScreen;
            NotificationPosition = _platformSettingsService.NotificationSettings.NotificationPosition;
            NotificationVisibleMaxCount = _platformSettingsService.NotificationSettings.NotificationVisibleMaxCount;
        }

        public string SettingsPath { get; }

        public bool? NotificationIsActive {
            get => _notificationIsActive;
            set => this.RaiseAndSetIfChanged(ref _notificationIsActive, value);
        }

        public NotificationScreen? NotificationScreen {
            get => _notificationScreen;
            set => this.RaiseAndSetIfChanged(ref _notificationScreen, value);
        }

        public NotificationPosition? NotificationPosition {
            get => _notificationPosition;
            set => this.RaiseAndSetIfChanged(ref _notificationPosition, value);
        }

        public int? NotificationVisibleMaxCount {
            get => _notificationVisibleMaxCount;
            set => this.RaiseAndSetIfChanged(ref _notificationVisibleMaxCount, value);
        }

        public override void SaveSettings() {
            NotificationSettings notificationSettings
                = _platformSettingsService.NotificationSettings;

            notificationSettings.IsActive = NotificationIsActive;
            notificationSettings.NotificationScreen = NotificationScreen;
            notificationSettings.NotificationPosition = NotificationPosition;
            notificationSettings.NotificationVisibleMaxCount = NotificationVisibleMaxCount;
        }
    }
}
