using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using pyRevitLabs.Json;

using RevitClashDetective.Models.Interfaces;
using RevitClashDetective.Models.Visiter;

namespace RevitClashDetective.Models.Value {
    internal class ElementIdParamValue : ParamValue<string> {

        public ElementIdParamValue() : base() {

        }

        [JsonConstructor]
        public ElementIdParamValue(int[] categories, string value, string stringValue) : base(value, stringValue) {
            Categories = categories;
        }

        public int[] Categories { get; }

        public override FilterRule GetFilterRule(IVisiter visiter, Document doc, RevitParam param) {
            if(Categories is null) {
                throw new ArgumentNullException(nameof(Categories));
            }
            var paramId = GetParamId(doc, param);
            if(paramId == ElementId.InvalidElementId)
                return null;

            if(visiter is EqualsVisiter || visiter is NotEqualsVisister) {
                var value = new FilteredElementCollector(doc)
                .WherePasses(new ElementMulticategoryFilter(Categories.Select(item => new ElementId(item)).ToArray()))
                .Select(item => GetValue(item, param))
                .Where(item => !item.IsNull())
                .Select(item => doc.GetElement(item))
                .FirstOrDefault(item => item.Name.Equals(TValue, StringComparison.CurrentCultureIgnoreCase));
                if(value == null)
                    return null;
                return visiter.Create(paramId, value?.Id);
            }

            return visiter.Create(paramId, TValue);
        }

        private ElementId GetValue(Element item, RevitParam param) {
            if(item.IsExistsParam(param)) {
                return item.GetParamValueOrDefault<ElementId>(param);
            } else {
                var typeId = item.GetTypeId();
                if(typeId != ElementId.InvalidElementId) {
                    var type = item.Document.GetElement(typeId);
                    if(type.IsExistsParam(param))
                        return type.GetParamValueOrDefault<ElementId>(param);
                }
            }
            return null;
        }

        public override void SetParamValue(Element element, string paramName) {
            if(element.IsExistsParam(paramName)) {
                element.SetParamValue(paramName, TValue);
            }
        }
    }
}
