using RevitFinishingWalls.ViewModels;
using RevitFinishingWalls.Views;

namespace RevitFinishingWalls.Services {
    internal class RichErrorMessageService {
        private readonly ErrorViewModel _errorViewModel;
        private readonly ErrorWindow _errorWindow;

        public RichErrorMessageService(ErrorViewModel errorViewModel, ErrorWindow errorWindow) {
            _errorViewModel = errorViewModel ?? throw new System.ArgumentNullException(nameof(errorViewModel));
            _errorWindow = errorWindow ?? throw new System.ArgumentNullException(nameof(errorWindow));
        }


        public void ShowErrorMessage(string message) {
            _errorViewModel.Message = message;
            _errorWindow.DataContext = _errorViewModel;
            _errorWindow.Show();
        }
    }
}
