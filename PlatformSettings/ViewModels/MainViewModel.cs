using System.Collections.ObjectModel;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using PlatformSettings.Factories;

namespace PlatformSettings.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly ISettingsViewModelFactory _settingsViewModelFactory;

        private string _errorText;
        private SettingsViewModel _setting;
        private ObservableCollection<SettingsViewModel> _settings;

        public MainViewModel(ISettingsViewModelFactory settingsViewModelFactory) {
            _settingsViewModelFactory = settingsViewModelFactory;

            LoadViewCommand = RelayCommand.Create(LoadView);
            ApplyViewCommand = RelayCommand.Create(ApplyView);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand ApplyViewCommand { get; }

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
            Settings = new ObservableCollection<SettingsViewModel>() {
                _settingsViewModelFactory.Create<SettingsViewModel>(0, 0, "Общие"),
                _settingsViewModelFactory.Create<ExtensionsSettingsViewModel>(1, 0, "Расширения"),
            };

            Setting = Settings[1];
        }

        private void ApplyView() {
            foreach(SettingsViewModel settingsViewModel in Settings) {
                settingsViewModel.SaveSettings();
            }
        }
    }
}