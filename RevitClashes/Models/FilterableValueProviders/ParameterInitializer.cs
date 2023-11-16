using System;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.SystemParams;
using dosymep.Revit;

namespace RevitClashDetective.Models.FilterableValueProviders {
    internal class ParameterInitializer {
        public static RevitParam InitializeParameter(Document doc, ElementId id) {
            if(id.IsSystemId()) {
                return SystemParamsConfig.Instance.CreateRevitParam(doc, (BuiltInParameter) id.GetIdValue());
            } else {
                var element = doc.GetElement(id);
                if(element is SharedParameterElement sharedParameterElement) {
                    return SharedParamsConfig.Instance.CreateRevitParam(doc, sharedParameterElement.Name);
                }
                if(element is ParameterElement parameterElement) {
                    return ProjectParamsConfig.Instance.CreateRevitParam(doc, parameterElement.Name);
                }
            }
            throw new ArgumentException(nameof(id), $"Невозможно преобразовать в параметр элемент с id = {id} в документе \"{doc.Title}\".");
        }
    }
}
