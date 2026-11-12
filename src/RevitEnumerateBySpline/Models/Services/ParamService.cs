using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Bim4Everyone.SystemParams;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitEnumerateBySpline.Models.Interfaces;

namespace RevitEnumerateBySpline.Models.Services;

internal class ParamService : IParamService {
    private readonly RevitRepository _revitRepository;
    private readonly SystemPluginConfig _systemPluginConfig;
    private readonly IRevitParamFactory _revitParamFactory;
    private readonly ILocalizationService _localizationService;
    private readonly ICollection<ElementId> _allParamElementIds;

    public ParamService(
        RevitRepository revitRepository,
        SystemPluginConfig systemPluginConfig,
        IRevitParamFactory revitParamFactory,
        ILocalizationService localizationService) {

        _revitRepository = revitRepository;
        _systemPluginConfig = systemPluginConfig;
        _revitParamFactory = revitParamFactory;
        _localizationService = localizationService;

        _allParamElementIds = GetAllParamElementIds();

        AllSpatialRevitParams = GetAllRevitParams().AsReadOnly();

        DefaultSpatialNameParam = GetDefaultNameParam();
        DefaultSpatialNumberParam = GetDefaultNumberParam();
        DefaultDependentSearchParam = GetDefaultNameParam();
        DefaultDependentParam = SharedParamsConfig.Instance.RoomArea;
    }

    public IReadOnlyList<RevitParam> AllSpatialRevitParams { get; }
    public RevitParam? DefaultSpatialNameParam { get; }
    public RevitParam? DefaultSpatialNumberParam { get; }
    public RevitParam? DefaultDependentParam { get; }
    public RevitParam? DefaultDependentSearchParam { get; }
    
    public string GetParamValue(SpatialModel spatialModel, RevitParam? filterParam) {
        if(spatialModel.SpatialElement is not { } spatialElement || filterParam is null) {
            return GetNoValueText();
        }

        if(IsLevelParameter(filterParam)) {
            return GetLevelValue(spatialModel, spatialElement);
        }

        if(!spatialElement.IsExistsParam(filterParam)) {
            return GetNoValueText();
        }

        TryGetParam(filterParam, spatialElement, out var parameter);
        
        if(parameter is null || !parameter.HasValue) {
            return GetNoValueText();
        }

        return parameter.StorageType switch {
            StorageType.String => spatialElement.GetParamValueOrDefault(filterParam.Name, GetNoValueText()),
            StorageType.Double => GetDoubleValue(spatialElement, filterParam, parameter),
            StorageType.Integer => GetIntegerValue(spatialElement, filterParam, parameter),
            StorageType.ElementId => GetElementIdValue(spatialElement, filterParam),
            _ => GetNoValueText()
        };
    }
    
    // Метод получения параметра по RevitParam и Element
    public bool TryGetParam(RevitParam revitParam, Element? element, out Parameter? param) {
        try {
            param = revitParam.GetParam(element);
            return param != null;
        } catch {
            param = null;
            return false;
        }
    }

    // Определяет параметр уровня.
    private static bool IsLevelParameter(RevitParam revitParam) {
        return revitParam is SystemParam systemParam
               && (systemParam.SystemParamId == BuiltInParameter.ROOM_LEVEL_ID
                   || systemParam.SystemParamId == BuiltInParameter.LEVEL_PARAM);
    }

    // Возвращает имя уровня помещения.
    private string GetLevelValue(SpatialModel spatialModel, SpatialElement spatialElement) {
        return spatialElement.Document.GetElement(spatialElement.LevelId)?.Name
               ?? spatialModel.LevelName
               ?? GetNoValueText();
    }

    // Форматирует числовой параметр.
    private string GetDoubleValue(SpatialElement spatialElement, RevitParam revitParam, Parameter parameter) {
        double value = spatialElement.GetParamValueOrDefault(revitParam.Name, double.NaN);

        if(double.IsNaN(value)) {
            return GetNoValueText();
        }
        
        try {
            string formattedValue = UnitFormatUtils.Format(
                spatialElement.Document.GetUnits(),
                parameter.Definition.GetDataType(),
                value,
                false);

            if (!string.IsNullOrWhiteSpace(formattedValue)) {
                return formattedValue;
            }
        }
        catch (Autodesk.Revit.Exceptions.ArgumentException) {
            // Formatting is not supported for this specification.
        }

        return value.ToString(CultureInfo.CurrentCulture);
    }

    
    // Форматирует целочисленный параметр.
    private string GetIntegerValue(SpatialElement spatialElement, RevitParam revitParam, Parameter parameter) {
        int? value = spatialElement.GetParamValueOrDefault<int?>(revitParam.Name);

        if (value is null) {
            return GetNoValueText();
        }

        if (parameter.Definition.GetDataType() == SpecTypeId.Boolean.YesNo) {
            string key = value == 0
                ? "ParamService.No"
                : "ParamService.Yes";

            return _localizationService.GetLocalizedString(key);
        }

        try {
            string formattedValue = UnitFormatUtils.Format(
                spatialElement.Document.GetUnits(),
                parameter.Definition.GetDataType(),
                value.Value,
                false);

            if (!string.IsNullOrWhiteSpace(formattedValue))
                return formattedValue;
        }
        catch (Autodesk.Revit.Exceptions.ArgumentException) {
            // Formatting is not supported for this specification.
        }

        return value.Value.ToString(CultureInfo.CurrentCulture);
    }


    // Возвращает значение параметра-ссылки.
    private string GetElementIdValue(SpatialElement spatialElement, RevitParam revitParam) {
        var elementId = spatialElement.GetParamValueOrDefault(revitParam.Name, ElementId.InvalidElementId);
        if(elementId == ElementId.InvalidElementId || elementId.ToString() == "-1") {
            return GetNoValueText();
        }

        string? elementName = spatialElement.Document.GetElement(elementId)?.Name;
        
        if(elementName is  null) {
            return GetNoValueText();
        }
        
        return !string.IsNullOrWhiteSpace(elementName) 
            ? elementName 
            : elementId.ToString();
    }

    // Возвращает текст отсутствующего значения.
    private string GetNoValueText() {
        return _localizationService.GetLocalizedString("ParamService.NoValue");
    }

    // Собирает идентификаторы параметров помещений.
    private ICollection<ElementId> GetAllParamElementIds() {
        var categoryId = new ElementId(BuiltInCategory.OST_Rooms);

        return ParameterFilterUtilities
            .GetFilterableParametersInCommon(_revitRepository.Document, [categoryId]);
    }

    // Находит параметр имени по умолчанию.
    private RevitParam? GetDefaultNameParam() {
        return AllSpatialRevitParams
            .OfType<SystemParam>()
            .FirstOrDefault(x => x.SystemParamId == _systemPluginConfig.SystemRoomNameParamId);
    }
    
    // Находит параметр номера по умолчанию.
    private RevitParam? GetDefaultNumberParam() {
        return AllSpatialRevitParams
            .OfType<SystemParam>()
            .FirstOrDefault(x => x.SystemParamId == _systemPluginConfig.SystemRoomNumberParamId);
    }

    // Создаёт список доступных параметров Revit.
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
