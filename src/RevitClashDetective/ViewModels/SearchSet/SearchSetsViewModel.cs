using System;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.FilterGenerators;
using RevitClashDetective.Models.FilterModel;

namespace RevitClashDetective.ViewModels.SearchSet {
    internal class SearchSetsViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly SearchSetViewModel _straightSearchSet;
        private readonly SearchSetViewModel _invertedSearchSet;
        private SearchSetViewModel _searchSet;

        public SearchSetsViewModel(RevitRepository revitRepository, Filter filter) {
            _revitRepository = revitRepository;

            Filter = filter;
            _straightSearchSet = new SearchSetViewModel(_revitRepository, Filter, new StraightRevitFilterGenerator());
            _invertedSearchSet = new SearchSetViewModel(_revitRepository, Filter, new InvertedRevitFilterGenerator());

            SearchSet = _straightSearchSet;

            InversionChangedCommand = RelayCommand.Create(InversionChanged);
            ShowSetCommand = RelayCommand.Create(ShowSet);
            CloseCommand = RelayCommand.Create(Close);
        }

        public bool Inverted { get; set; }

        public ICommand InversionChangedCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand ShowSetCommand { get; }

        public SearchSetViewModel SearchSet {
            get => _searchSet;
            set => this.RaiseAndSetIfChanged(ref _searchSet, value);
        }

        public Filter Filter { get; }

        private void InversionChanged() {
            if(Inverted) {
                SearchSet = _invertedSearchSet;
            } else {
                SearchSet = _straightSearchSet;
            }
        }

        private void Close() {
            Action action = () => {
                var command = new CreateFiltersCommand();
                command.ExecuteCommand(_revitRepository.UiApplication, Filter.Name);
            };
            _revitRepository.DoAction(action);
        }

        private void ShowSet() {
            try {
                Filter filter;
                if(Inverted) {
                    filter = _invertedSearchSet.Filter;
                } else {
                    filter = _straightSearchSet.Filter;
                }
                _revitRepository.ShowFilter(filter);
            } catch(InvalidOperationException ex) {
                _revitRepository.ShowErrorMessage(ex.Message);
            }
        }
    }
}
