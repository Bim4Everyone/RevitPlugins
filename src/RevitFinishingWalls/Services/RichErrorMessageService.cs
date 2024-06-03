using System.Collections.Generic;

using RevitFinishingWalls.ViewModels;
using RevitFinishingWalls.Views;

namespace RevitFinishingWalls.Services {
    internal class RichErrorMessageService {
        private readonly ErrorWindowViewModel _errorViewModel;
        private readonly ErrorWindow _errorWindow;

        public RichErrorMessageService(ErrorWindowViewModel errorViewModel, ErrorWindow errorWindow) {
            _errorViewModel = errorViewModel ?? throw new System.ArgumentNullException(nameof(errorViewModel));
            _errorWindow = errorWindow ?? throw new System.ArgumentNullException(nameof(errorWindow));
        }


        public void ShowErrorWindow(ICollection<RoomErrorsViewModel> errors) {
            _errorViewModel.LoadRooms(errors);
            _errorWindow.DataContext = _errorViewModel;
            _errorWindow.Show();
        }
    }
}
