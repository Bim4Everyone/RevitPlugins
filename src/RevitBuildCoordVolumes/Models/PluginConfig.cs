using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SharedParams;

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
    // Утвержденные карты параметров, которые используются в проектах разных дисциплин
    private readonly List<ParamMap> _allParamMaps = [
        new ParamMap {
            Type = ParamType.DescriptionParam,
            SourceParam = SharedParamsConfig.Instance.Description,
            TargetParam = SharedParamsConfig.Instance.Description
        },
        new ParamMap {
            Type = ParamType.BlockParam,
            SourceParam = SharedParamsConfig.Instance.BuildingWorksBlock,
            TargetParam = SharedParamsConfig.Instance.BuildingWorksBlock
        },
        new ParamMap {
            Type = ParamType.SectionParam,
            SourceParam = SharedParamsConfig.Instance.BuildingWorksSection,
            TargetParam = SharedParamsConfig.Instance.BuildingWorksSection
        },
        new ParamMap {
            Type = ParamType.FloorParam,
            SourceParam = SharedParamsConfig.Instance.BuildingWorksLevel,
            TargetParam = SharedParamsConfig.Instance.BuildingWorksLevel
        },
        new ParamMap {
            Type = ParamType.FloorDEParam,
            SourceParam = SharedParamsConfig.Instance.BuildingWorksLevelCurrency,
            TargetParam = SharedParamsConfig.Instance.BuildingWorksLevelCurrency
        },
    ];

    // Категории, обрабатываемые плагином в качестве основы для построения
    public ICollection<BuiltInCategory> SlabCategories => [
        BuiltInCategory.OST_StructuralFoundation,
        BuiltInCategory.OST_Floors];

    // ID категории для построения DirectShape
    public ElementId ElementIdDirectShape => new(BuiltInCategory.OST_GenericModel);

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

    // Временное имя параметра низа зоны
    public string BottomAreaParamName => "ФОП_Отметка низа СМР";

    // Временное имя параметра низа зоны
    public string UpAreaParamName => "ФОП_Отметка верха СМР";

    // Наиболее оптимальная сторона фигуры для поиска плиты в миллиметрах
    public double SquareSide => 200;

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
    /// Метод получения карты параметров
    /// </summary>
    public List<ParamMap> GetDefaultParamMaps() {
        return _allParamMaps;
    }
}
