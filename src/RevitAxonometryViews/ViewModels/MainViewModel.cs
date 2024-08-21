using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitAxonometryViews.Models;
using RevitAxonometryViews.Views;

namespace RevitAxonometryViews.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;

        private string _errorText;
        private string _saveProperty;

        public MainViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public string SaveProperty {
            get => _saveProperty;
            set => this.RaiseAndSetIfChanged(ref _saveProperty, value);
        }

        private bool CanAcceptView() {
            if(string.IsNullOrEmpty(SaveProperty)) {
                ErrorText =  "MainWindow.HelloCheck";
                return false;
            }

            ErrorText = null;
            return true;
        }


        public ObservableCollection<HvacSystem> GetDataSource() {
            return _revitRepository.GetHvacSystems();
        }

        public void ShowWindow() {
            MainWindow mainWindow = new MainWindow(this);
            mainWindow.Show();
        }
    }
}
