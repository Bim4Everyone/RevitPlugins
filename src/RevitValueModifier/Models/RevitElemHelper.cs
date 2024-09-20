using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitValueModifier.Models {
    internal class RevitElemHelper {
        private readonly Document _document;

        public RevitElemHelper(Document document) {
            _document = document;
        }

        public List<RevitElem> GetRevitElements(List<Element> elements, List<ElementId> parameterIds) {
            return elements
                .Select(element => GetRevitElem(element, parameterIds))
                .ToList();
        }


        public RevitElem GetRevitElem(Element element, List<ElementId> parameterIds) {
            List<ParamValuePair> paramValuePairList = element.Parameters
                .Cast<Parameter>()
                .Where(p => parameterIds.Contains(p.Id))
                .Select(p => GetParamValuePair(p))
                .ToList();
            return new RevitElem(element, paramValuePairList);
        }


        private ParamValuePair GetParamValuePair(Parameter parameter) {
            RevitParameter revitParameter = new RevitParameter();
            string value;

            var def = parameter.Definition;
            revitParameter.Name = def.Name;
            revitParameter.IsBuiltin = def.GetBuiltInParameter() != BuiltInParameter.INVALID;

            switch(parameter.StorageType) {
                case StorageType.Double:
                    revitParameter.StorageType = StorageType.Double;
                    value = parameter.AsValueString();
                    break;
                case StorageType.ElementId:
                    revitParameter.StorageType = StorageType.ElementId;
                    ElementId id = parameter.AsElementId();

#if REVIT_2024_OR_GREATER
                    if(id.Value >= 0) {
                        value = _document.GetElement(id).Name;
                    } else {
                        value = id.Value.ToString();
                    }
#else
                    if(id.IntegerValue >= 0) {
                        value = _document.GetElement(id).Name;
                    } else {
                        value = id.IntegerValue.ToString();
                    }
#endif
                    break;
                case StorageType.Integer:
                    revitParameter.StorageType = StorageType.Integer;
                    if(SpecTypeId.Boolean.YesNo == parameter.Definition.GetDataType()) {
                        if(parameter.AsInteger() == 0) {
                            value = "False";
                        } else {
                            value = "True";
                        }
                    } else {
                        value = parameter.AsInteger().ToString();
                    }
                    break;
                case StorageType.String:
                    revitParameter.StorageType = StorageType.String;
                    value = parameter.AsString();
                    break;
                default:
                    value = "Значение неизвестно";
                    break;
            }

            return new ParamValuePair(revitParameter, value);
        }
    }
}
