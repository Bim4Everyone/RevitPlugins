using System.Collections.ObjectModel;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPlatformSettings.Factories;
using RevitPlatformSettings.ViewModels.Settings;

using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;

namespace RevitPlatformSettings.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private string _errorText;
        
        private ObservableCollection<INavigationViewItem> _navigationViewItems;
        private ObservableCollection<INavigationViewItem> _footerNavigationViewItems;

        public MainViewModel() {
            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(ApplyView);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public ObservableCollection<INavigationViewItem> NavigationViewItems {
            get => _navigationViewItems;
            set => _navigationViewItems = value;
        }

        public ObservableCollection<INavigationViewItem> FooterNavigationViewItems {
            get => _footerNavigationViewItems;
            set => _footerNavigationViewItems = value;
        }

        private void LoadView() {
            
        }

        private void ApplyView() {
            // foreach(SettingsViewModel settingsViewModel in Settings) {
            //     settingsViewModel.SaveSettings();
            // }
        }
    }
}
