using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.FilterGenerators;
using RevitClashDetective.Models.FilterModel;

namespace RevitClashDetective.ViewModels.SearchSet {
    internal class SearchSetsViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private SearchSetViewModel _selectedSearchSet;

        public SearchSetsViewModel(RevitRepository revitRepository, Filter filter) {
            _revitRepository = revitRepository;
            Filter = filter;
            SearchSets = new List<SearchSetViewModel>() {
                new SearchSetViewModel(_revitRepository, Filter, new StraightRevitFilterGenerator(), "Поисковый набор"),
                new SearchSetViewModel(_revitRepository, Filter, new InvertedRevitFilterGenerator(), "Инвертированный поисковый набор")
            };
            SelectedSearchSet = SearchSets.FirstOrDefault();
        }

        public SearchSetViewModel SelectedSearchSet { 
            get => _selectedSearchSet; 
            set => this.RaiseAndSetIfChanged(ref _selectedSearchSet, value); 
        }

        public Filter Filter { get; }
        public List<SearchSetViewModel> SearchSets { get; set; }
    }
}