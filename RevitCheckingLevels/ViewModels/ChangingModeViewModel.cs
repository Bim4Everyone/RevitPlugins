using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCheckingLevels.Services;

namespace RevitCheckingLevels.ViewModels
{
    internal class ChangingModeViewModel : BaseViewModel {
        private bool _isSelectCheckingLevel;

        public ChangingModeViewModel(INavigationService navigationService) {
            Navigation = navigationService;
            IsSelectCheckingLevel = true;
            ViewCommand = new RelayCommand(ChangeMode);
        }

        public ICommand ViewCommand { get; }
        public INavigationService Navigation { get; }

        public bool IsSelectCheckingLevel {
            get => _isSelectCheckingLevel;
            set => this.RaiseAndSetIfChanged(ref _isSelectCheckingLevel, value);
        }

        private void ChangeMode(object p) {
            if(IsSelectCheckingLevel) {
                Navigation.NavigateTo<CheckingLevelsViewModel>();
            } else {
                Navigation.NavigateTo<CheckingLinkLevelsViewModel>();
            }
        }
    }
}