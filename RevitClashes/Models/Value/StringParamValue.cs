
using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.Value {
    internal class StringParamValue : ParamValue<string> {
        public StringParamValue() {

        }

        public StringParamValue(string value, string stringValue) : base(value, stringValue) {

        }

        public StringParamValue(string value) : base(value) {

        }

        public override FilterRule GetFilterRule(IVisiter visiter, Document doc, RevitParam param) {
            var paramId = GetParamId(doc, param);
            if(!paramId.IsNotNull())
                return null;
            return visiter.Create(paramId, TValue);
        }

        public override void SetParamValue(Element element, string paramName) {
            if(element.IsExistsParam(paramName)) {
                element.SetParamValue(paramName, TValue);
            }
        }
    }
}
