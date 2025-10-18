using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

using RevitSetCoordParams.Models.Interfaces;

namespace RevitSetCoordParams.Models;
internal class ParamFactory {
    private readonly IParamAvailabilityService _paramAvailabilityService;

    public ParamFactory(IParamAvailabilityService paramAvailabilityService) {
        _paramAvailabilityService = paramAvailabilityService;
    }

    public RevitParam CreateRevitParam(Document document, string paramName) {

        var def = _paramAvailabilityService.GetDefinitionByName(paramName);
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

    //public RevitParam CreateRevitParam2(Document document, string paramName) {

    //    var def = _paramAvailabilityService.GetDefinitionByName(paramName);
    //    var id = def.GetElementId();

    //    if(!id.IsSystemId()) {
    //        var element = document.GetElement(id);
    //        if(element is SharedParameterElement) {
    //            RevitParam revitParam = SharedParamsConfig.Instance.CreateRevitParam(document, element.Name);

    //            return revitParam;
    //        } else {
    //            RevitParam revitParam = ProjectParamsConfig.Instance.CreateRevitParam(document, element.Name);
    //            return revitParam;
    //        }
    //    }
    //    return null;
    //}
}
