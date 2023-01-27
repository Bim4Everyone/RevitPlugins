using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.UI;

using dosymep.WPF.ViewModels;

using RevitCheckingLevels.Models;
using RevitCheckingLevels.Services;

namespace RevitCheckingLevels.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private string _errorText;

        public MainViewModel(INavigationService navigationService) {
            Navigation = navigationService;
            Navigation.NavigateTo<ChangingModeViewModel>();
        }

        public INavigationService Navigation { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }
    }
}