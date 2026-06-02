using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.SystemParams;
using dosymep.Revit;

namespace RevitRoundingOfAreas.Models;

internal class ParamService {
    private readonly RevitRepository _revitRepository;
    private readonly SystemPluginConfig _systemPluginConfig;
    private readonly ICollection<ElementId> _allParamElementIds;

    public ParamService(
        RevitRepository revitRepository,
        SystemPluginConfig systemPluginConfig) {

        _revitRepository = revitRepository;
        _systemPluginConfig = systemPluginConfig;

        _allParamElementIds = GetAllParamElementIds();

        AllRevitParams = GetAllRevitParams().AsReadOnly();

        DefaultSourceParam = GetDefaultSourceParam();
        DefaultTargetParam = GetDefaultTargetParam();
    }

    public IReadOnlyList<RevitParam> AllRevitParams { get; }
    public RevitParam DefaultSourceParam { get; }
    public RevitParam DefaultTargetParam { get; }

    private ICollection<ElementId> GetAllParamElementIds() {
        var categoryId = new ElementId(BuiltInCategory.OST_Rooms);

        return ParameterFilterUtilities
            .GetFilterableParametersInCommon(_revitRepository.Document, [categoryId]);
    }

    private RevitParam GetDefaultSourceParam() {
        return AllRevitParams
            .OfType<SystemParam>()
            .FirstOrDefault(x => x.SystemParamId == _systemPluginConfig.SystemRoomAreaParamId);
    }

    private RevitParam GetDefaultTargetParam() {
        return AllRevitParams
            .OfType<SharedParam>()
            .FirstOrDefault(x => x.Name.Equals(_systemPluginConfig.RoomAreaParam.Name));
    }

    private List<RevitParam> GetAllRevitParams() {
        if(_allParamElementIds.Count == 0) {
            return [];
        }

        List<RevitParam> parameters = [];

        foreach(var elementId in _allParamElementIds) {
            var revitParam = CreateRevitParam(elementId);

            if(revitParam is null) {
                continue;
            }

            if(IsSupportedStorageType(revitParam)) {
                parameters.Add(revitParam);
            }
        }

        return parameters;
    }

    private RevitParam CreateRevitParam(ElementId elementId) {
        if(elementId.IsSystemId()) {
            var builtInParameter = elementId.AsBuiltInParameter();
            return SystemParamsConfig.Instance.CreateRevitParam(_revitRepository.Document, builtInParameter);
        }

        var element = _revitRepository.Document.GetElement(elementId);

        return element is null
            ? null
            : element is SharedParameterElement
            ? SharedParamsConfig.Instance.CreateRevitParam(_revitRepository.Document, element.Name)
            : ProjectParamsConfig.Instance.CreateRevitParam(_revitRepository.Document, element.Name);
    }

    private static bool IsSupportedStorageType(RevitParam revitParam) {
        return revitParam.StorageType == StorageType.Double;
    }
}
