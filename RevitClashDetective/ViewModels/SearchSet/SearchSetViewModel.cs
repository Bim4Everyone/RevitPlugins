using System.Collections.Generic;
using System.Linq;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.FilterGenerators;
using RevitClashDetective.Models.FilterModel;

namespace RevitClashDetective.ViewModels.SearchSet {
    internal class SearchSetViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;

        public SearchSetViewModel(RevitRepository revitRepository, Filter filter, RevitFilterGenerator generator) {
            _revitRepository = revitRepository;
            FilterGenerator = generator;
            Filter = filter;
            InitializeGrid();
        }

        public string Message { get; set; }
        public RevitFilterGenerator FilterGenerator { get; set; }
        public Filter Filter { get; set; }
        public GridControlViewModel Grid { get; set; }
        private void InitializeGrid() {
            var elements = new List<ElementModel>();
            var docInfos = _revitRepository.DocInfos;
            foreach(DocInfo docInfo in docInfos) {
                var filter = Filter.GetRevitFilter(docInfo.Doc, FilterGenerator);
                var elems = _revitRepository.GetFilteredElements(docInfo.Doc, Filter.CategoryIds, filter).Where(item => item != null && item.IsValidObject).ToList();
                elements.AddRange(elems.Select(item => new ElementModel(item, docInfo.Transform)));
            }

            Grid = new GridControlViewModel(_revitRepository, Filter, elements);
        }
    }
}
