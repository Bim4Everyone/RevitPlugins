using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitValueModifier.Models {
    internal class RevitParameterUtils {
        private readonly List<StorageType> _neededStorageTypes = new List<StorageType>() {
                StorageType.Integer,
                StorageType.Double,
                StorageType.String
            };


        public RevitParameterUtils(List<RevitElem> revitElems) {
            RevitElems = revitElems;
        }


        public List<RevitElem> RevitElems { get; }

        public List<RevitParameter> GetIntersectedParameters() {
            if(RevitElems is null || RevitElems.Count() == 0) { return new List<RevitParameter>(); }

            // Т.к. списки нужно сравнивать, чтобы найти перечень параметров, которые содержатся
            // одновременно у всех элементов, заполняем список параметрами первого элемента
            var collectionForIntersect = RevitElems.First().GetElementParameters();
            if(RevitElems.Count() == 1) { return collectionForIntersect.Select(p => new RevitParameter(p)).ToList(); }

            for(int i = 1; i < RevitElems.Count(); i++) {
                var paramsOfCurrentElem = RevitElems[i].GetElementParameters();
                collectionForIntersect = collectionForIntersect.Intersect(paramsOfCurrentElem, new RevitParameterComparerById());
            }

            return collectionForIntersect
                .Where(p => FilterParameterByStorageType(p))
                .Select(p => new RevitParameter(p))
                .OrderBy(rP => rP.Name)
                .ToList();
        }

        private bool FilterParameterByStorageType(Parameter parameter) {
            return _neededStorageTypes.Contains(parameter.StorageType) ? true : false;
        }


        internal List<RevitParameter> GetNotReadOnlyParameters(List<RevitParameter> revitParameters) {
            List<RevitParameter> notReadOnly = new List<RevitParameter>();
            foreach(RevitParameter revitParameter in revitParameters) {
                if(!revitParameter.IsReadOnly) {
                    notReadOnly.Add(revitParameter);
                }
            }
            return notReadOnly;
        }


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
