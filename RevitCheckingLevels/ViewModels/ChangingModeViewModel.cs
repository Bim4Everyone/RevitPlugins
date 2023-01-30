using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCheckingLevels.Models;
using RevitCheckingLevels.Services;

namespace RevitCheckingLevels.ViewModels {
    internal class ChangingModeViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;

        private string _errorText;
        private bool _isSelectCheckingLevel;
        private LinkTypeViewModel _linkType;

        public ChangingModeViewModel(RevitRepository revitRepository, INavigationService navigationService) {
            _revitRepository = revitRepository;

            Navigation = navigationService;
            IsSelectCheckingLevel = true;

            ViewLoadCommand = new RelayCommand(Load);
            ViewCommand = new RelayCommand(ChangeMode, CanChangeMode);
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

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
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

            LinkType = LinkTypes.FirstOrDefault();
        }

        private void ChangeMode(object p) {
            if(IsSelectCheckingLevel) {
                Navigation.NavigateTo<CheckingLevelsViewModel>();
            } else {
                Navigation.NavigateTo<CheckingLinkLevelsViewModel>();
            }
        }

        private bool CanChangeMode(object p) {
            if(!IsSelectCheckingLevel) {
                if(LinkType == null) {
                    ErrorText = "Выберите координационный файл.";
                    return false;
                }

                if(!LinkType.IsLinkLoaded) {
                    ErrorText = "Загрузите координационный файл.";
                    return false;
                }
            }

            ErrorText = null;
            return true;
        }
    }
}