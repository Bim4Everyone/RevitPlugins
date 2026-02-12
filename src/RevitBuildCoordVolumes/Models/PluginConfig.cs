using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.SimpleServices;

using pyRevitLabs.Json;

using RevitBuildCoordVolumes.Models.Enums;
using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.Models;

internal class PluginConfig : ProjectConfig<RevitSettings> {

    [JsonIgnore]
    public override string ProjectConfigPath { get; set; }

    [JsonIgnore]
    public override IConfigSerializer Serializer { get; set; }

    public static PluginConfig GetPluginConfig(IConfigSerializer configSerializer) {
        return new ProjectConfigBuilder()
            .SetSerializer(configSerializer)
            .SetPluginName(nameof(RevitBuildCoordVolumes))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(PluginConfig) + ".json")
            .Build<PluginConfig>();
    }
}

internal class RevitSettings : ProjectSettings {
    public override string ProjectName { get; set; }
    public ConfigSettings ConfigSettings { get; set; }
}

internal class SystemPluginConfig {
    // Параметр описания зон
    private static readonly RevitParam _descriptionParam = SharedParamsConfig.Instance.BuildingWorksDescription;
    // Параметр блока СМР
    private static readonly RevitParam _blockParam = SharedParamsConfig.Instance.BuildingWorksBlock;
    // Параметр секции СМР
    private static readonly RevitParam _sectionParam = SharedParamsConfig.Instance.BuildingWorksSection;
    // Параметр этажа СМР
    private static readonly RevitParam _floorParam = SharedParamsConfig.Instance.BuildingWorksLevel;
    // Параметр этажа СМР - денежная единица
    private static readonly RevitParam _floorDEParam = SharedParamsConfig.Instance.BuildingWorksLevelCurrency;
    // Параметр зоны СМР
    private static readonly RevitParam _zoneParam = SharedParamsConfig.Instance.BuildingWorksZone;
    // Параметр низа СМР
    private static readonly RevitParam _bottomZoneParam = SharedParamsConfig.Instance.BuildingWorksMarkBottom;
    // Параметр верха СМР
    private static readonly RevitParam _topZoneParam = SharedParamsConfig.Instance.BuildingWorksMarkTop;
    // Параметр верха СМР
    private static readonly RevitParam _volumeParam = SharedParamsConfig.Instance.SizeVolumeBuildingWorks;

    // Утвержденные карты параметров, которые используются в расширенном алгоритме
    private readonly List<ParamMap> _advancedParamMaps = [
        new ParamMap {
            Type = ParamType.DescriptionParam,
            TypeValue = ParamTypeValue.TextParam,
            SourceParam = _descriptionParam,
            TargetParam = _descriptionParam
        },
        new ParamMap {
            Type = ParamType.BlockParam,
            TypeValue = ParamTypeValue.TextParam,
            SourceParam = _blockParam,
            TargetParam = _blockParam
        },
        new ParamMap {
            Type = ParamType.SectionParam,
            TypeValue = ParamTypeValue.TextParam,
            SourceParam = _sectionParam,
            TargetParam = _sectionParam
        },
        new ParamMap {
            Type = ParamType.FloorParam,
            TypeValue = ParamTypeValue.TextParam,
            SourceParam = _floorParam,
            TargetParam = _floorParam
        },
        new ParamMap {
            Type = ParamType.FloorDEParam,
            TypeValue = ParamTypeValue.CurrencyParam,
            SourceParam = _floorDEParam,
            TargetParam = _floorDEParam
        },
        new ParamMap {
            Type = ParamType.ZoneParam,
            TypeValue = ParamTypeValue.TextParam,
            SourceParam = _zoneParam,
            TargetParam = _zoneParam
        },
        new ParamMap {
            Type = ParamType.VolumeParam,
            TypeValue = ParamTypeValue.ValueParam,
            SourceParam = null,
            TargetParam = _volumeParam
        },
    ];

    // Утвержденные карты параметров, которые используются в простом алгоритме
    private readonly List<ParamMap> _simpleParamMaps = [
        new ParamMap {
            Type = ParamType.BottomZoneParam,
            TypeValue = ParamTypeValue.TextParam,
            SourceParam = _bottomZoneParam,
            TargetParam = null
        },
        new ParamMap {
            Type = ParamType.TopZoneParam,
            TypeValue = ParamTypeValue.TextParam,
            SourceParam = _topZoneParam,
            TargetParam = null
        }
    ];

    private readonly ILocalizationService _localizationService;

    public SystemPluginConfig(ILocalizationService localizationService) {
        _localizationService = localizationService;
        SlopeLineName = _localizationService.GetLocalizedString("SystemPluginConfig.SlopeLineName");
    }

    // Категории, обрабатываемые плагином в качестве основы для построения
    public ICollection<BuiltInCategory> SlabCategories => [
        BuiltInCategory.OST_StructuralFoundation,
        BuiltInCategory.OST_Floors];

    // ID категории для построения DirectShape
    public ElementId ElementIdDirectShape => new(BuiltInCategory.OST_GenericModel);

    // Строка для поиска линии наклона. По другому идентифицировать эту линию невозможно
    public string SlopeLineName { get; private set; }

    // Строка в имени уровня, определяющая кровлю
    public string DefaultStringRoof => "К";

    // Число для параметра FloorDE, определяющий кровлю
    public double DefaultStringRoofDE => 100;

    // Строка в имени уровня, определяющая автостоянку / подвальный этаж
    public string DefaultStringParking => "П";

    // Строка в имени уровня, определяющая этаж
    public string DefaultStringFloor => "этаж";

    // Самое распространенное имя типа зоны для записи координат
    public string DefaultStringTypeZone => "Координаты СМР";

    // Наиболее оптимальная сторона фигуры для поиска плиты в миллиметрах
    public double SquareSideMm => 100;

    // Угол поворота квадрата по умолчанию
    public double SquareAngleDeg => 0;

    // Части имен типоразмеров плит перекрытий КР
    public List<string> DefaultSlabTypeNames => [
        "(КР)",
        "КЖ",
        "кж",
        "ЖБ",
        "жб",
        "Железобетон",
        "железобетон",
        "Монолит",
        "монолит"];

    /// <summary>
    /// Метод получения карты параметров для расширенного алгоритма
    /// </summary>
    public List<ParamMap> GetAdvancedParamMaps() {
        return _advancedParamMaps;
    }

    /// <summary>
    /// Метод получения карты параметров для простого алгоритма
    /// </summary>
    public List<ParamMap> GetSimpleParamMaps() {
        var simpleParamMaps = _simpleParamMaps;
        return _advancedParamMaps
            .Concat(simpleParamMaps)
            .ToList();
    }

    /// <summary>
    /// Метод получения всех параметров для загрузки в проект
    /// </summary>
    public List<RevitParam> GetAllParameters() {
        return _advancedParamMaps.Concat(_simpleParamMaps)
            .Select(pm => pm.SourceParam ?? pm.TargetParam)
            .Where(p => p != null)
            .ToList();
    }
}
