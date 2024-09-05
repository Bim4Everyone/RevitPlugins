using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitValueModifier.Models {
    internal class RevitElemHelper {
        private readonly Document _document;

        public RevitElemHelper(Document document) {
            _document = document;
        }

        public RevitElem GetRevitElem(Element element) {

            List<ParamValuePair> paramList = new List<ParamValuePair>();
            foreach(Parameter parameter in element.Parameters) {

                paramList.Add(GetParameterInfo(parameter));
            }

            return new RevitElem(element, paramList);
        }


        private ParamValuePair GetParameterInfo(Parameter parameter) {

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
                    if(id.IntegerValue >= 0) {
                        value = _document.GetElement(id).Name;
                    } else {
                        value = id.IntegerValue.ToString();
                    }
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
