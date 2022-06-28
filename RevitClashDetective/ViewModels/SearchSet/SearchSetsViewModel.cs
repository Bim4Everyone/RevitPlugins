using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.FilterGenerators;
using RevitClashDetective.Models.FilterModel;

namespace RevitClashDetective.ViewModels.SearchSet {
    internal class SearchSetsViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private SearchSetViewModel _searchSet;
        private SearchSetViewModel _straightSearchSet;
        private SearchSetViewModel _invertedSearchSet;

        public SearchSetsViewModel(RevitRepository revitRepository, Filter filter) {
            _revitRepository = revitRepository;

            Filter = filter;
            _straightSearchSet = new SearchSetViewModel(_revitRepository, Filter, new StraightRevitFilterGenerator());
            _invertedSearchSet = new SearchSetViewModel(_revitRepository, Filter, new InvertedRevitFilterGenerator());

            SearchSet = _straightSearchSet;

            InversionChangedCommand = new RelayCommand(InversionChanged);
        }

        public bool Inverted { get; set; }

        public ICommand InversionChangedCommand { get; }

        public SearchSetViewModel SearchSet {
            get => _searchSet;
            set => this.RaiseAndSetIfChanged(ref _searchSet, value);
        }

        public Filter Filter { get; }

        private void InversionChanged(object p) {
            if(Inverted) {
                SearchSet = _invertedSearchSet;
            } else {
                SearchSet = _straightSearchSet;
            }
        }
    }
}