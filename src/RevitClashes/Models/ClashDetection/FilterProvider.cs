using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models.Extensions;
using RevitClashDetective.Models.FilterGenerators;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.ClashDetection {
    internal class FilterProvider : IProvider {

        private readonly Filter _filter;
        private readonly ICollection<ElementId> _elementsToFilter;

        public FilterProvider(Document doc, Filter filterElement, Transform transform, params ElementId[] elementsToFilter) {
            Doc = doc ?? throw new ArgumentNullException(nameof(doc));
            _filter = filterElement ?? throw new ArgumentNullException(nameof(filterElement));
            MainTransform = transform ?? throw new ArgumentNullException(nameof(transform));

            _elementsToFilter = elementsToFilter;
        }

        public Document Doc { get; }
        public Transform MainTransform { get; }

        public List<Element> GetElements() {
            var categories = _filter.CategoryIds.Select(item => item.AsBuiltInCategory()).ToList();

            var elements = GetFilteredElementCollector()
                .WherePasses(new ElementMulticategoryFilter(categories))
                .WherePasses(_filter.GetRevitFilter(Doc, new StraightRevitFilterGenerator()))
                .WhereElementIsNotElementType()
                .Where(item => item.get_Geometry(new Options()) != null)
                .ToList();
            return elements;
        }

        public List<Solid> GetSolids(Element element) {
            return element.GetSolids();
        }

        private FilteredElementCollector GetFilteredElementCollector() {
            return _elementsToFilter != null && _elementsToFilter.Count > 0
                ? new FilteredElementCollector(Doc, _elementsToFilter)
                : new FilteredElementCollector(Doc);
        }
    }
}
