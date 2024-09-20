using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitValueModifier.Models {
    internal class RevitParameterHelper {
        public RevitParameterHelper() { }

        public List<ForgeTypeId> GetIntersectedParameterIds(List<Element> elements) {

            if(elements is null || elements.Count() == 0) { return new List<ForgeTypeId>(); }

            var intersectedParameters = GetParametersFromElem(elements.First());

            for(int i = 1; i < elements.Count(); i++) {
                var paramsOfCurrentElem = GetParametersFromElem(elements[i]);
                intersectedParameters = intersectedParameters.Intersect(paramsOfCurrentElem, new RevitParameterComparerById());
            }

            return intersectedParameters.Select(p => p.GetTypeId()).ToList();
        }

        private IEnumerable<Parameter> GetParametersFromElem(Element element) {
            return element.Parameters.Cast<Parameter>();
        }




        public List<Parameter> GetIntersectedParameters(List<Element> elements) {

            if(elements is null || elements.Count() == 0) { return new List<Parameter>(); }

            var intersectedParameters = GetParametersFromElem(elements.First());

            for(int i = 1; i < elements.Count(); i++) {
                var paramsOfCurrentElem = GetParametersFromElem(elements[i]);
                intersectedParameters = intersectedParameters.Intersect(paramsOfCurrentElem, new RevitParameterComparerById());
            }

            return intersectedParameters.ToList();
        }
    }
}
