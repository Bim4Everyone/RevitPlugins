using System;
using System.Linq;
using System.Windows.Input;

using dosymep.Revit;
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
        private readonly InvertedVisibilityRevitFilterGenerator _invertedVisibilityFilterGenerator;
        private SearchSetViewModel _searchSet;

        public SearchSetsViewModel(RevitRepository revitRepository, Filter filter) {
            _revitRepository = revitRepository;

            Filter = filter;
            _straightSearchSet = new SearchSetViewModel(_revitRepository, Filter, new StraightRevitFilterGenerator());
            _invertedSearchSet = new SearchSetViewModel(_revitRepository, Filter, new InvertedRevitFilterGenerator());
            _invertedVisibilityFilterGenerator = new InvertedVisibilityRevitFilterGenerator();

            SearchSet = _straightSearchSet;
            Name = filter.Name;

            InversionChangedCommand = RelayCommand.Create(InversionChanged);
            ShowSetCommand = RelayCommand.Create(ShowSet);
            CloseCommand = RelayCommand.Create(Close);
        }

        public string Name { get; }

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
            ShowSet();
        }

        private void Close() {
            Action action = () => {
                var command = new CreateFiltersCommand();
                command.ExecuteCommand(_revitRepository.UiApplication, Filter.Name);
            };
            _revitRepository.DoAction(action);
        }

        private void ShowSet() {
            SearchSetViewModel invertedSelectedSet;
            RevitFilterGenerator generator;
            if(Inverted) {
                invertedSelectedSet = _straightSearchSet;
                generator = invertedSelectedSet.FilterGenerator;
            } else {
                invertedSelectedSet = _invertedSearchSet;
                generator = _invertedVisibilityFilterGenerator;
            }
            _revitRepository.ShowElements(
                invertedSelectedSet.Filter.GetRevitFilter(_revitRepository.Doc, generator),
                invertedSelectedSet.Filter.CategoryIds.Select(c => c.AsBuiltInCategory()).ToHashSet(),
                out string error);
            if(!string.IsNullOrWhiteSpace(error)) {
                _revitRepository.ShowErrorMessage(error);
            }
        }
    }
}
