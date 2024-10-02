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
                ParamStorageType = document.get_TypeOfStorage(bInParameter);
                ParamName = LabelUtils.GetLabelFor(bInParameter);
                ParamTypeName = RevitParamType.SystemParameter;
                //ParamTypeName =  _localizationService.GetLocalizedString("MainWindow.SystemParameter");
            } else {
                ParameterElement paramElement = _document.GetElement(parameterId) as ParameterElement;
                ParamName = paramElement.Name;
                ParamStorageType = paramElement.GetStorageType();
                if(paramElement.IsSharedParam()) {
                    ParamTypeName = RevitParamType.SharedParameter;
                    //ParamTypeName = _localizationService.GetLocalizedString("MainWindow.SharedParameter");
                } else {
                    ParamTypeName = RevitParamType.ProjectParameter;
                    //ParamTypeName = _localizationService.GetLocalizedString("MainWindow.ProjectParameter");
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
