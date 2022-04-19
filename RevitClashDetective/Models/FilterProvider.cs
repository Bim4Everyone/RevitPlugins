using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitClashDetective.Models {
    internal class FilterProvider : IProvider {
        private readonly ParameterFilterElement _filterElement;

        public FilterProvider(ParameterFilterElement filterElement) {
            _filterElement = filterElement;
        }

        public List<Element> GetElements(Document doc) {
            return new FilteredElementCollector(doc)
                .WherePasses(_filterElement.GetElementFilter())
                .ToList();
        }
    }
}
