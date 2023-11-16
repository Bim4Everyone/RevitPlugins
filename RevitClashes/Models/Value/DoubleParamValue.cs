
using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.Value {
    internal class DoubleParamValue : ParamValue<double> {
        public DoubleParamValue() {

        }
        public DoubleParamValue(double value, string stringValue) : base(value, stringValue) { }

        public DoubleParamValue(double value) : base(value) { }

        public override FilterRule GetFilterRule(IVisiter visiter, Document doc, RevitParam param) {
            var paramId = GetParamId(doc, param);
            if(paramId == ElementId.InvalidElementId)
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
