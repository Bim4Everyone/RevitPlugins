using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitValueModifier.Models {
    internal class RevitParameter {

        private readonly Document _document;

        public RevitParameter(ElementId parameterId, Document document) {
            _document = document;

            // Проверяем является ли параметр встроенным параметром
            if(parameterId.IsSystemId()) {
                BInParameter = (BuiltInParameter) parameterId.GetIdValue();
                ParamName = LabelUtils.GetLabelFor(BInParameter);
                Id = parameterId;
                IsBuiltin = true;
                IsShared = false;
            } else {
                ParamElement = _document.GetElement(parameterId) as ParameterElement;
                ParamName = ParamElement.Name;
                Id = parameterId;
                IsBuiltin = false;
                IsShared = ParamElement.IsSharedParam();
            }
        }


        public string ParamName { get; set; }
        public BuiltInParameter BInParameter { get; set; }
        public ParameterElement ParamElement { get; set; }
        public ElementId Id { get; set; }
        public bool IsBuiltin { get; set; }
        public bool IsShared { get; set; }


        //public StorageType StorageType { get; set; }
        //public bool IsReadOnly { get; set; }

        public override string ToString() => $"{ParamName}";
    }
}
