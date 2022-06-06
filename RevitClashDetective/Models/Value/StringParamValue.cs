
using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.Value {
    internal class StringParamValue : ParamValue<string> {
        public StringParamValue(string value, string stringValue) : base(value, stringValue) {

        }
        public override FilterRule GetFilterRule(IVisiter visiter, Document doc, RevitParam param) {
            var paramId = GetParamId(doc, param);
            if(paramId == ElementId.InvalidElementId)
                return null;
            return visiter.Create(paramId, TValue);
        }

    }
}
