using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

namespace RevitValueModifier.Models {
    internal class RevitParameter {

        private readonly Document _document;
        private readonly ILocalizationService _localizationService;

        public RevitParameter(ElementId parameterId, Document document, ILocalizationService localizationService) {
            _document = document;
            _localizationService = localizationService;

            if(parameterId.IsSystemId()) {
                BuiltInParameter bInParameter = (BuiltInParameter) parameterId.GetIdValue();
                ParamName = LabelUtils.GetLabelFor(bInParameter);
                ParamTypeName = _localizationService.GetLocalizedString("MainWindow.SystemParameter");
            } else {
                ParameterElement paramElement = _document.GetElement(parameterId) as ParameterElement;
                ParamName = paramElement.Name;
                if(paramElement.IsSharedParam()) {
                    ParamTypeName = _localizationService.GetLocalizedString("MainWindow.SharedParameter");
                } else {
                    ParamTypeName = _localizationService.GetLocalizedString("MainWindow.ProjectParameter");
                }
            }
            Id = parameterId;
        }

        public string ParamName { get; set; }
        public ElementId Id { get; set; }
        public string ParamTypeName { get; set; }
    }
}
