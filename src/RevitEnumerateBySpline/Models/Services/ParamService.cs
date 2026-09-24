using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Bim4Everyone.SystemParams;

namespace RevitEnumerateBySpline.Models.Services;

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

        DefaultFilterParam = GetDefaultFilterParam();
    }

    public IReadOnlyList<RevitParam> AllRevitParams { get; }
    public RevitParam? DefaultFilterParam { get; }
    
    public string GetParamValue(SpatialModel spatialModel, RevitParam? filterParam) {
        return spatialModel.SpatialElement.GetParamValueOrDefault<string>(filterParam, "Нет значения");
    }

    private ICollection<ElementId> GetAllParamElementIds() {
        var categoryId = new ElementId(BuiltInCategory.OST_Rooms);

        return ParameterFilterUtilities
            .GetFilterableParametersInCommon(_revitRepository.Document, [categoryId]);
    }

    private RevitParam? GetDefaultFilterParam() {
        return AllRevitParams
            .OfType<SystemParam>()
            .FirstOrDefault(x => x.SystemParamId == _systemPluginConfig.SystemRoomNameParamId);
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

            parameters.Add(revitParam);
        }
        return parameters;
    }

    
}
