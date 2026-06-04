using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Bim4Everyone.SystemParams;

namespace RevitRoundingOfAreas.Models;

internal class ParamService {
    private readonly RevitRepository _revitRepository;
    private readonly SystemPluginConfig _systemPluginConfig;
    private readonly IRevitParamFactory _revitParamFactory;
    private readonly ICollection<ElementId> _allParamElementIds;

    public ParamService(
        RevitRepository revitRepository,
        SystemPluginConfig systemPluginConfig,
        IRevitParamFactory revitParamFactory) {

        _revitRepository = revitRepository;
        _systemPluginConfig = systemPluginConfig;
        _revitParamFactory = revitParamFactory;

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
            var revitParam = _revitParamFactory.Create(_revitRepository.Document, elementId);

            if(revitParam is null) {
                continue;
            }

            if(IsSupportedStorageType(revitParam)) {
                parameters.Add(revitParam);
            }
        }
        return parameters;
    }

    private static bool IsSupportedStorageType(RevitParam revitParam) {
        return revitParam.StorageType == StorageType.Double;
    }
}
