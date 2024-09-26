namespace RevitValueModifier.Models {
    internal class RevitParameterUtils {



        //        public ParamValuePair GetParamValuePair(Parameter parameter) {
        //            RevitParameter revitParameter = new RevitParameter();
        //            string value;

        //            var def = parameter.Definition;
        //            revitParameter.Name = def.Name;
        //            revitParameter.IsBuiltin = def.GetBuiltInParameter() != BuiltInParameter.INVALID;

        //            switch(parameter.StorageType) {
        //                case StorageType.Double:
        //                    revitParameter.StorageType = StorageType.Double;
        //                    value = parameter.AsValueString();
        //                    break;
        //                case StorageType.ElementId:
        //                    revitParameter.StorageType = StorageType.ElementId;
        //                    ElementId id = parameter.AsElementId();

        //#if REVIT_2024_OR_GREATER
        //                    if(id.Value >= 0) {
        //                        value = _document.GetElement(id).Name;
        //                    } else {
        //                        value = id.Value.ToString();
        //                    }
        //#else
        //                    if(id.IntegerValue >= 0) {
        //                        value = _document.GetElement(id).Name;
        //                    } else {
        //                        value = id.IntegerValue.ToString();
        //                    }
        //#endif
        //                    break;
        //                case StorageType.Integer:
        //                    revitParameter.StorageType = StorageType.Integer;
        //                    if(SpecTypeId.Boolean.YesNo == parameter.Definition.GetDataType()) {
        //                        if(parameter.AsInteger() == 0) {
        //                            value = "False";
        //                        } else {
        //                            value = "True";
        //                        }
        //                    } else {
        //                        value = parameter.AsInteger().ToString();
        //                    }
        //                    break;
        //                case StorageType.String:
        //                    revitParameter.StorageType = StorageType.String;
        //                    value = parameter.AsString();
        //                    break;
        //                default:
        //                    value = "Значение неизвестно";
        //                    break;
        //            }

        //            return new ParamValuePair(revitParameter, value);
        //        }
    }
}
