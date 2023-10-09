using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using pyRevitLabs.Json;

using RevitClashDetective.Models.FilterGenerators;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.FilterModel {
    internal class Filter {
        public Filter(RevitRepository revitRepository) {
            RevitRepository = revitRepository;
        }

        public string Name { get; set; }
        public Set Set { get; set; }

        [JsonIgnore]
        public RevitRepository RevitRepository { get; set; }
#if REVIT_2023_OR_LESS
        public List<int> CategoryIds { get; set; }
#else
        public List<long> CategoryIds { get; set; }
#endif

        public IEnumerable<IFilterableValueProvider> GetProviders() {
            return Set.GetProviders().Distinct();
        }

        public ElementFilter GetRevitFilter(Document doc, RevitFilterGenerator generator) {
            Set.FilterGenerator = generator;
            Set.Generate(doc);
            return generator.Generate();
        }
    }
}
