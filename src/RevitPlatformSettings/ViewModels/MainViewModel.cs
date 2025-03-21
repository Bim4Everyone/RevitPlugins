using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPlatformSettings.Factories;
using RevitPlatformSettings.ViewModels.Settings;
using RevitPlatformSettings.Views.Pages;

using Wpf.Ui.Abstractions;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;

namespace RevitPlatformSettings.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly INavigationViewPageProvider _pageProvider;

        private readonly Type[] _pages = new[] {
            typeof(AboutSettingsPage), 
            typeof(ExtensionsSettingsPage), typeof(GeneralSettingsPage),
            typeof(RevitParamsSettingsPage), typeof(TelemetrySettingsPage)
        };
        
        private string _errorText;

        public MainViewModel(INavigationViewPageProvider pageProvider) {
            _pageProvider = pageProvider;
            
            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(ApplyView);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        private void LoadView() {
            // pass
        }

        private void ApplyView() {
            IEnumerable<SettingsViewModel> settings = _pages
                .Select(item => _pageProvider.GetPage(item))
                .Where(item => item != null)
                .OfType<Page>()
                .Select(item => item.DataContext)
                .OfType<SettingsViewModel>();

            foreach(SettingsViewModel settingsViewModel in settings) {
                settingsViewModel.SaveSettings();
            }
        }
    }
}
