using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

namespace RevitCheckingLevels.Services {
    internal interface INavigationService {
        BaseViewModel CurrentView { get; }
        void NavigateTo<T>() where T : BaseViewModel;
    }

    internal class NavigationService : BaseViewModel, INavigationService {
        private BaseViewModel _currentView;
        private readonly Func<Type, BaseViewModel> _viewModelFactory;

        public NavigationService(Func<Type, BaseViewModel> viewModelFactory) {
            _viewModelFactory = viewModelFactory;
        }

        public BaseViewModel CurrentView {
            get => _currentView;
            private set => this.RaiseAndSetIfChanged(ref _currentView, value);
        }

        public void NavigateTo<T>() where T : BaseViewModel {
            CurrentView = _viewModelFactory(typeof(T));
        }
    }
}