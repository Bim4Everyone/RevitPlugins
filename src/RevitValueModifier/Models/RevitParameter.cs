using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitValueModifier.Models {
    internal class RevitParameter {

        private readonly Document _document;

        public RevitParameter(ElementId parameterId, Document document) {
            _document = document;

            if(parameterId.IsSystemId()) {
                BuiltInParameter bInParameter = (BuiltInParameter) parameterId.GetIdValue();
                ParamStorageType = document.get_TypeOfStorage(bInParameter);
                ParamName = LabelUtils.GetLabelFor(bInParameter);
                ParamTypeName = RevitParamType.SystemParameter;
            } else {
                ParameterElement paramElement = _document.GetElement(parameterId) as ParameterElement;
                ParamName = paramElement.Name;
                ParamStorageType = paramElement.GetStorageType();
                if(paramElement.IsSharedParam()) {
                    ParamTypeName = RevitParamType.SharedParameter;
                } else {
                    ParamTypeName = RevitParamType.ProjectParameter;
                }
            }
            Id = parameterId;
        }

        public string ParamName { get; set; }
        public ElementId Id { get; set; }
        public RevitParamType ParamTypeName { get; set; }
        public StorageType ParamStorageType { get; set; }

        public override string ToString() => $"{ParamName}";
    }
}
