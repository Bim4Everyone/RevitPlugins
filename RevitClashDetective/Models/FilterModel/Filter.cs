using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.FilterGenerators;

namespace RevitClashDetective.Models.FilterModel {
    internal class Filter {
        private readonly RevitRepository _revitRepository;

        public Set Set { get; set; }
        public List<int> CategoryIds { get; set; }
        public string Name { get; set; }

        public Filter(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        public void CreateRevitFilter() {
            var generator = new RevitFilterGenerator();
            Set.FilterGenerator = generator;
            Set.Generate();
            var elementFilter = generator.Generate();
            var ids = CategoryIds.Select(item => new ElementId(item));
            _revitRepository.CreateFilter(ids, elementFilter, Name);
        }
    }
}
