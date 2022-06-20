using dosymep.WPF.ViewModels;

namespace RevitWindowGapPlacement.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private BaseViewModel _currentViewModel;

        public BaseViewModel CurrentViewModel {
            get => _currentViewModel;
            set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
        }
    }
}