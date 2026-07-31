using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

using RevitLintelsManager.Models.Settings;

namespace RevitLintelsManager.Models.Configs;

/// <summary>
/// Класс конфигурации плагина.
/// </summary>
internal class PluginConfig : ProjectConfig<RevitSettings> {
    [JsonIgnore]
    public override string ProjectConfigPath { get; set; }
    
    [JsonIgnore]
    public override IConfigSerializer Serializer { get; set; }
    
    public static PluginConfig GetPluginConfig(IConfigSerializer configSerializer) {
        return new ProjectConfigBuilder()
            .SetSerializer(configSerializer)
            .SetPluginName(nameof(RevitLintelsManager))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(PluginConfig) + ".json")
            .Build<PluginConfig>();
    }
}

internal class RevitSettings : ProjectSettings {
    
    public override string ProjectName { get; set; }
    
    public LintelManagerSettings LintelManagerSettings { get; set; }
}

internal class SystemPluginConfig {
    // Коллекция категорий допустимых семейств перемычек
    public readonly IEnumerable<BuiltInCategory> DefaultLintelCats = [
        BuiltInCategory.OST_GenericModel
    ];
    
    // Коллекция категорий допустимых семейств, над которыми могут устанавливаться перемычки
    public readonly IEnumerable<BuiltInCategory> DefaultOpeningCats = [
        BuiltInCategory.OST_Doors,    
        BuiltInCategory.OST_Windows
    ];
    
    public string DefaultLintelFamilyName  => "ОбщМд_Перемычка";
    public string LintelWidthParamName  => "Ширина проема";
    public string LintelThicknessParamName  => "Толщина стены";
    public string LintelRightOffsetParamName  => "Опирание справа";
    public string LintelLeftOffsetParamName  => "Опирание слева";
    public string LintelRightCornerParamName  => "Уголок справа";
    public string LintelLeftCornerParamName  => "Уголок слева";
    public string LintelRightWeldingParamName  => "Крепления на металл справа";
    public string LintelLeftWeldingParamName  => "Крепления на металл слева";
    public IEnumerable<string> DefaultOpeningFamilyNames  => [
        "Двр_Двр_Однп", 
        "Двр_Двр_Двуп",
        "Двр_Про_Проем",
        "Двр_Люк_Ревизия",
        "Окн_Окн_1 Ств Бизнес",
        "Окн_Про_Проем",
        "Окн_Отв_Прямоуг_Стена"
    ];
    public string OpeningHeightParamName  => LabelUtils.GetLabelFor(BuiltInParameter.INSTANCE_HEAD_HEIGHT_PARAM);
    public string OpeningWidthParamName  => LabelUtils.GetLabelFor(BuiltInParameter.FAMILY_WIDTH_PARAM);
    public string DefaultLintelConfigRuleName  => "A101";
    public string DefaultLintelFixParamName  => "ФОП_Фиксация решения";
    public double DefaultMinimalHeightAboveLintelMm  => 100;
    public IEnumerable<string> DefaultStructureWallTypeNames  => [];
    public string DefaultPhase  => "Основная планировка";
    
}
