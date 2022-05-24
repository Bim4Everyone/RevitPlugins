using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.FilterGenerators;
using pyRevitLabs.Json;
using System.IO;

namespace RevitClashDetective.Models.FilterModel {
    internal class Filter {
        public Filter(RevitRepository revitRepository) {
            RevitRepository = revitRepository;
        }

        public string Name { get; set; }
        public Set Set { get; set; }

        [JsonIgnore]
        public RevitRepository RevitRepository { get; set; }
        public List<int> CategoryIds { get; set; }

        public ElementFilter GetRevitFilter(Document doc) {
            var generator = new RevitFilterGenerator();
            Set.FilterGenerator = generator;
            Set.Generate(doc);
            return generator.Generate();
        }
    }
}
