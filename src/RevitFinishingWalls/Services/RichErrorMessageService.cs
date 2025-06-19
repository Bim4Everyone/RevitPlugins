using System.Collections.Generic;

using Ninject;
using Ninject.Syntax;

using RevitFinishingWalls.ViewModels;
using RevitFinishingWalls.Views;

namespace RevitFinishingWalls.Services {
    internal class RichErrorMessageService {
        private readonly IResolutionRoot _resolutionRoot;

        public RichErrorMessageService(IResolutionRoot resolutionRoot) {
            _resolutionRoot = resolutionRoot ?? throw new System.ArgumentNullException(nameof(resolutionRoot));
        }


        public void ShowErrorWindow(ICollection<RoomErrorsViewModel> errors) {
            var viewModel = _resolutionRoot.Get<ErrorWindowViewModel>();
            var window = _resolutionRoot.Get<ErrorWindow>();
            viewModel.LoadRooms(errors);
            window.DataContext = viewModel;
            window.Show();
        }
    }
}
