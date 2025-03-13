using System.Collections.ObjectModel;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPlatformSettings.Factories;
using RevitPlatformSettings.ViewModels.Settings;

namespace RevitPlatformSettings.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly ILocalizationService _localizationService;
        private readonly ISettingsViewModelFactory _settingsViewModelFactory;

        private string _errorText;
        private SettingsViewModel _setting;
        private ObservableCollection<SettingsViewModel> _settings;

        public MainViewModel(
            ILocalizationService localizationService,
            ISettingsViewModelFactory settingsViewModelFactory) {
            _localizationService = localizationService;
            _settingsViewModelFactory = settingsViewModelFactory;

            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(ApplyView);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public SettingsViewModel Setting {
            get => _setting;
            set => this.RaiseAndSetIfChanged(ref _setting, value);
        }

        public ObservableCollection<SettingsViewModel> Settings {
            get => _settings;
            set => this.RaiseAndSetIfChanged(ref _settings, value);
        }

        private void LoadView() {
            var root = _settingsViewModelFactory.Create<SettingsViewModel>(
                _localizationService.GetLocalizedString("SettingsNode.Title"));

            root.Settings = new ObservableCollection<SettingsViewModel>() {
                _settingsViewModelFactory.Create<GeneralSettingsViewModel>(
                    _localizationService.GetLocalizedString("GeneralSettings.Title")),
                _settingsViewModelFactory.Create<ExtensionsSettingsViewModel>(
                    _localizationService.GetLocalizedString("ExtensionSettings.Title")),
                _settingsViewModelFactory.Create<RevitParamsSettingsViewModel>(
                    _localizationService.GetLocalizedString("RevitParamsSettings.Title")),
                _settingsViewModelFactory.Create<TelemetrySettingsViewModel>(
                    _localizationService.GetLocalizedString("TelemetrySettings.Title")),
                _settingsViewModelFactory.Create<AboutSettingsViewModel>(
                    _localizationService.GetLocalizedString("AboutSettings.Title")),
            };

            Settings = new ObservableCollection<SettingsViewModel>() {root};
            //Setting = Settings[1];
        }

        private void ApplyView() {
            foreach(SettingsViewModel settingsViewModel in Settings) {
                settingsViewModel.SaveSettings();
            }
        }
    }
}
