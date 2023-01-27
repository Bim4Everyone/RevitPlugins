using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCheckingLevels.Services;
using RevitCheckingLevels.Views;

namespace RevitCheckingLevels.ViewModels {
    internal class CheckingLinkLevelsViewModel : BaseViewModel {
        private readonly MainWindow _mainWindow;
        private readonly INavigationService _navigationService;

        public CheckingLinkLevelsViewModel(MainWindow mainWindow, INavigationService navigationService) {
            _mainWindow = mainWindow;
            _navigationService = navigationService;

            ViewCommand = new RelayCommand(Execute);
            ViewLoadCommand = new RelayCommand(Load);
            ViewHomeCommand = new RelayCommand(ToHome);
        }

        public ICommand ViewCommand { get; }
        public ICommand ViewLoadCommand { get; }
        public ICommand ViewHomeCommand { get; }

        private void Execute(object p) {
            _mainWindow.DialogResult = true;
        }

        private void Load(object p) {
            MessageBox.Show("aaaa");
        }

        private void ToHome(object p) {
            _navigationService.NavigateTo<ChangingModeViewModel>();
        }
    }
}
