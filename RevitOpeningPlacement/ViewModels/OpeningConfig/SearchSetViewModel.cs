using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.FilterGenerators;

using RevitClashDetective.Models.FilterModel;

using RevitOpeningPlacement.Models;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig {
    /// <summary>
    /// Модель представления для фильтра по элементам
    /// </summary>
    internal class SearchSetViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;

        public SearchSetViewModel(RevitRepository revitRepository, Filter filter, RevitFilterGenerator generator) {
            _revitRepository = revitRepository;
            FilterGenerator = generator;
            Filter = filter;
            InitializeGrid();
        }

        public RevitFilterGenerator FilterGenerator { get; set; }
        public Filter Filter { get; set; }
        public GridControlViewModel Grid { get; set; }
        private void InitializeGrid() {
            var elements = new List<Element>();
            var doc = _revitRepository.Doc;
            var filter = Filter.GetRevitFilter(doc, FilterGenerator);
            var elems = _revitRepository.GetFilteredElements(doc, Filter.CategoryIds, filter).Where(item => item != null && item.IsValidObject).ToList();
            elements.AddRange(elems);

            Grid = new GridControlViewModel(_revitRepository, Filter, elements);
        }
    }
}
