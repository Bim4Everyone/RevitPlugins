using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.SystemParams;
using dosymep.Revit;

namespace RevitRoundingOfAreas.Models;
internal class ParamService {

    private readonly RevitRepository _revitRepository;

    public ParamService(RevitRepository revitRepository) {
        _revitRepository = revitRepository;

    }

    public List<RevitParam> AllRevitParams => GetAllRevitParams();

    public List<RevitParam> GetAllRevitParams() {
        var categoryId = new ElementId(BuiltInCategory.OST_Rooms);
        var filterableParameters = ParameterFilterUtilities
            .GetFilterableParametersInCommon(_revitRepository.Document, [categoryId]);

        if(filterableParameters.Count == 0) {
            return [];
        }

        List<RevitParam> listOfParameters = [];
        foreach(var elementId in filterableParameters) {
            if(elementId.IsSystemId()) {
                var param = elementId.AsBuiltInParameter();
                RevitParam revitParam = SystemParamsConfig.Instance.CreateRevitParam(_revitRepository.Document, param);
                if(revitParam.StorageType is StorageType.Double or StorageType.String) {
                    listOfParameters.Add(revitParam);
                }
            } else {
                var element = _revitRepository.Document.GetElement(elementId);
                if(element is SharedParameterElement) {
                    RevitParam revitParam = SharedParamsConfig.Instance.CreateRevitParam(_revitRepository.Document, element.Name);
                    if(revitParam.StorageType is StorageType.Double or StorageType.String) {
                        listOfParameters.Add(revitParam);
                    }
                } else {
                    RevitParam revitParam = ProjectParamsConfig.Instance.CreateRevitParam(_revitRepository.Document, element.Name);
                    if(revitParam.StorageType is StorageType.Double or StorageType.String) {
                        listOfParameters.Add(revitParam);
                    }
                }
            }
        }

        return listOfParameters;
    }
}
