using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCheckingLevels.Models;
using RevitCheckingLevels.Services;

namespace RevitCheckingLevels.ViewModels
{
    internal class ChangingModeViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;

        private bool _isSelectCheckingLevel;
        private LinkTypeViewModel _linkType;

        public ChangingModeViewModel(RevitRepository revitRepository, INavigationService navigationService) {
            _revitRepository = revitRepository;
            
            Navigation = navigationService;
            IsSelectCheckingLevel = true;

            ViewLoadCommand = new RelayCommand(Load);
            ViewCommand = new RelayCommand(ChangeMode);
        }

        public ICommand ViewCommand { get; }
        public ICommand ViewLoadCommand { get; }
        public INavigationService Navigation { get; }
        
        public ObservableCollection<LinkTypeViewModel> LinkTypes { get; } 
            = new ObservableCollection<LinkTypeViewModel>();

        public LinkTypeViewModel LinkType {
            get => _linkType;
            set => this.RaiseAndSetIfChanged(ref _linkType, value);
        }

        public bool IsSelectCheckingLevel {
            get => _isSelectCheckingLevel;
            set => this.RaiseAndSetIfChanged(ref _isSelectCheckingLevel, value);
        }

        private void Load(object p) {
            LinkTypes.Clear();
            foreach(RevitLinkType linkType in _revitRepository.GetRevitLinkTypes()) {
                LinkTypes.Add(new LinkTypeViewModel(linkType));
            }
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