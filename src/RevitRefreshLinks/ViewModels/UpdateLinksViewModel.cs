using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.ViewModels {
    internal class UpdateLinksViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly ILocalizationService _localizationService;
        private string _errorText;


        public UpdateLinksViewModel(
            RevitRepository revitRepository,
            ILocalizationService localizationService) {

            _revitRepository = revitRepository;
            _localizationService = localizationService;

            LinksToUpdate = new ObservableCollection<LinkToUpdateViewModel>();

            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
            SelectLocalSourceCommand = RelayCommand.Create(SelectLocalSource);
            SelectServerSourceCommand = RelayCommand.Create(SelectServerSource);
            SelectAllLinksCommand = RelayCommand.Create(SelectAllLinks, CanSelectAllLinks);
            UnselectAllLinksCommand = RelayCommand.Create(UnselectAllLinks, CanUnselectAllLinks);
            InvertSelectedLinksCommand = RelayCommand.Create(InvertSelectedLinks, CanInvertSelectedLinks);
        }


        public ICommand LoadViewCommand { get; }

        public ICommand AcceptViewCommand { get; }

        public ICommand SelectLocalSourceCommand { get; }

        public ICommand SelectServerSourceCommand { get; }

        public ICommand SelectAllLinksCommand { get; }

        public ICommand UnselectAllLinksCommand { get; }

        public ICommand InvertSelectedLinksCommand { get; }

        public ObservableCollection<LinkToUpdateViewModel> LinksToUpdate { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }


        private void LoadView() {
            var links = _revitRepository
                .GetExistingLinks()
                .Select(t => new LinkToUpdateViewModel(t))
                .OrderBy(t => t.Name);
            foreach(var link in links) {
                LinksToUpdate.Add(link);
            }
        }

        private void AcceptView() {
        }

        private bool CanAcceptView() {
            //if(true) {
            //    ErrorText = _localizationService.GetLocalizedString("UpdateLinksWindow.HelloCheck");
            //    return false;
            //}

            //ErrorText = null;
            return true;
        }

        private void InvertSelectedLinks() {
            throw new NotImplementedException();
        }

        private bool CanInvertSelectedLinks() {
            return true;
        }

        private void UnselectAllLinks() {
            throw new NotImplementedException();
        }

        private bool CanUnselectAllLinks() {
            return true;
        }

        private void SelectAllLinks() {
            throw new NotImplementedException();
        }

        private bool CanSelectAllLinks() {
            return true;
        }

        private void SelectServerSource() {
            throw new NotImplementedException();
        }

        private void SelectLocalSource() {
            throw new NotImplementedException();
        }
    }
}
