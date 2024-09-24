using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitValueModifier.Models {
    internal class RevitElem {
        public RevitElem(Element element) {
            Elem = element;
        }

        public IEnumerable<Parameter> GetElementParameters() {
            Parameters = Parameters is null ? Elem.Parameters.Cast<Parameter>().ToList() : Parameters;
            return Parameters;
        }

        private Element Elem { get; }
        private IEnumerable<Parameter> Parameters { get; set; }
        //public List<ParamValuePair> ParamValuePairs { get; }


        //public void GetParamValuePairs(RevitParameterHelper revitParameterHelper, List<ElementId> parameterIds) {

        //    List<ParamValuePair> paramValuePairList = Parameters
        //        .Where(p => parameterIds.Contains(p.Id))
        //        .Select(parameter => AddParamValuePair(parameter, revitParameterHelper))
        //        .ToList();
        //}


        //public void AddParamValuePair(Parameter parameter, RevitParameterHelper revitParameterHelper) {

        //    ParamValuePairs = revitParameterHelper.GetParamValuePair(parameter);
        //}
    }
}
