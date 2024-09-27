using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitValueModifier.Models {
    internal class RevitParameter {

        private readonly Document _document;

        public RevitParameter(ElementId parameterId, Document document) {
            _document = document;

            if(parameterId.IsSystemId()) {
                BuiltInParameter bInParameter = (BuiltInParameter) parameterId.GetIdValue();
                ParamName = LabelUtils.GetLabelFor(bInParameter);
                IsBuiltin = true;
                IsShared = false;
            } else {
                ParameterElement paramElement = _document.GetElement(parameterId) as ParameterElement;
                ParamName = paramElement.Name;
                IsBuiltin = false;
                IsShared = paramElement.IsSharedParam();
            }
            Id = parameterId;
        }

        public string ParamName { get; set; }
        public ElementId Id { get; set; }
        public bool IsBuiltin { get; set; }
        public bool IsShared { get; set; }

        public override string ToString() => $"{ParamName} (системный: {IsBuiltin}, общий: {IsShared})";

        //public BuiltInParameter BInParameter { get; set; }
        //public ParameterElement ParamElement { get; set; }
    }
}
