using System.Collections.ObjectModel;
using System.Windows.Input;

using dosymep.WPF.ViewModels;

namespace PlatformSettings.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private string _errorText;

        public MainViewModel() {
            Settings = new ObservableCollection<SettingsViewModel>() {
                new SettingsViewModel(0,-1, "Общие"),
                new ExtensionsSettingsViewModel(1,0, "Расширения"),
            };
        }
        
        public ICommand LoadViewCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }
        
        public ObservableCollection<SettingsViewModel> Settings { get; }
    }
}