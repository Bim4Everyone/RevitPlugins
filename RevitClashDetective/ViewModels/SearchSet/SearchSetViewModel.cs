using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
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
            var elements = new List<Element>();
            var docs = _revitRepository.GetDocuments().ToList();
            foreach(var doc in docs) {
                var filter = Filter.GetRevitFilter(doc, FilterGenerator);
                var elems = _revitRepository.GetFilteredElements(doc, Filter.CategoryIds, filter).Where(item => item != null && item.IsValidObject).ToList();
                elements.AddRange(elems);
            }

            Grid = new GridControlViewModel(_revitRepository, Filter, elements);
        }
    }
}