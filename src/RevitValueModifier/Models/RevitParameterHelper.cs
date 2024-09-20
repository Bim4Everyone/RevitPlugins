using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitValueModifier.Models {
    internal class RevitParameterHelper {
        public RevitParameterHelper() { }

        public List<ElementId> GetIntersectedParameterIds(List<Element> elements) {

            if(elements is null || elements.Count() == 0) { return new List<ElementId>(); }

            var intersectedParameters = GetParametersFromElem(elements.First());

            for(int i = 1; i < elements.Count(); i++) {
                var paramsOfCurrentElem = GetParametersFromElem(elements[i]);
                intersectedParameters = intersectedParameters.Intersect(paramsOfCurrentElem, new RevitParameterComparerById());
            }

            return intersectedParameters.Select(p => p.Id).ToList();
        }

        private IEnumerable<Parameter> GetParametersFromElem(Element element) {
            return element.Parameters.Cast<Parameter>();
        }
    }
}
