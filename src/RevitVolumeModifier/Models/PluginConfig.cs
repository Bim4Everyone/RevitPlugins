using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.SimpleServices;

using pyRevitLabs.Json;

using RevitVolumeModifier.Enums;
using RevitVolumeModifier.Interfaces;

namespace RevitVolumeModifier.Models;

internal class PluginConfig : ProjectConfig<RevitSettings> {

    [JsonIgnore]
    public override string ProjectConfigPath { get; set; }

    [JsonIgnore]
    public override IConfigSerializer Serializer { get; set; }

    public static PluginConfig GetPluginConfig(IConfigSerializer configSerializer) {
        return new ProjectConfigBuilder()
            .SetSerializer(configSerializer)
            .SetPluginName(nameof(RevitVolumeModifier))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(PluginConfig) + ".json")
            .Build<PluginConfig>();
    }
}

internal class RevitSettings : ProjectSettings {
    public override string ProjectName { get; set; }
    public List<ParamModel> ParamModels { get; set; }
}

internal class SystemPluginConfig {
    private readonly ILocalizationService _localizationService;
    private readonly IParamAvailabilityService _paramAvailabilityService;

    public SystemPluginConfig(ILocalizationService localizationService, IParamAvailabilityService paramAvailabilityService) {
        _localizationService = localizationService;
        _paramAvailabilityService = paramAvailabilityService;
    }

    // Категория моделей, с которыми работает плагин
    public BuiltInCategory ModelCategory => BuiltInCategory.OST_GenericModel;

    /// <summary>
    /// Метод получения списка всех параметров
    /// </summary>
    public List<ParamModel> GetDefaultParams() {
        return [
            GetDescriptionParam(),
            GetBlockParam(),
            GetSectionParam(),
            GetFloorParam(),
            GetFloorDEParam(),
            GetZoneParam(),
            GetVolumeParam()
        ];
    }

    // Метод получения параметра ФОП_Описание СМР     
    private ParamModel GetDescriptionParam() {
        var instance = SharedParamsConfig.Instance;
        var param = instance?.BuildingWorksDescription;
        return GetParam(param, ParamType.DescriptionParam);
    }

    // Метод получения параметра ФОП_Блок СМР     
    private ParamModel GetBlockParam() {
        var instance = SharedParamsConfig.Instance;
        var param = instance?.BuildingWorksBlock;
        return GetParam(param, ParamType.BlockParam);
    }

    // Метод получения параметра ФОП_Секция СМР    
    private ParamModel GetSectionParam() {
        var instance = SharedParamsConfig.Instance;
        var param = instance?.BuildingWorksSection;
        return GetParam(param, ParamType.SectionParam);
    }

    // Метод получения параметра ФОП_Этаж СМР    
    private ParamModel GetFloorParam() {
        var instance = SharedParamsConfig.Instance;
        var param = instance?.BuildingWorksLevel;
        return GetParam(param, ParamType.FloorParam);
    }

    // Метод получения параметра ФОП_Этаж СМР_ДЕ
    private ParamModel GetFloorDEParam() {
        var instance = SharedParamsConfig.Instance;
        var param = instance?.BuildingWorksLevelCurrency;
        return GetParam(param, ParamType.FloorDEParam);
    }

    // Метод получения параметра ФОП_Зона СМР    
    private ParamModel GetZoneParam() {
        var instance = SharedParamsConfig.Instance;
        var param = instance?.BuildingWorksZone;
        return GetParam(param, ParamType.ZoneParam);
    }

    // Метод получения параметра ФОП_РАЗМ_Объем СМР    
    private ParamModel GetVolumeParam() {
        var instance = SharedParamsConfig.Instance;
        var param = instance?.SizeVolumeBuildingWorks;
        return GetParam(param, ParamType.VolumeParam);
    }

    // Метод получения параметра
    private ParamModel GetParam(RevitParam revitParam, ParamType paramType) {
        return new ParamModel {
            RevitParam = revitParam,
            ParamType = paramType
        };
    }
}
