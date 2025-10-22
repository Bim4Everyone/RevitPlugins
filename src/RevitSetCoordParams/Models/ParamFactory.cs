using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

namespace RevitSetCoordParams.Models;
internal class ParamFactory {

    public RevitParam CreateRevitParam(Document document, Definition def) {

        var id = def.GetElementId();

        if(!id.IsSystemId()) {
            var element = document.GetElement(id);
            if(element is SharedParameterElement) {
                RevitParam revitParam = SharedParamsConfig.Instance.CreateRevitParam(document, element.Name);

                return revitParam;
            } else {
                RevitParam revitParam = ProjectParamsConfig.Instance.CreateRevitParam(document, element.Name);
                return revitParam;
            }
        }
        return null;
    }
}
