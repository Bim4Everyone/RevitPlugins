using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace RevitMarkPlacement.Models;

internal class PluginConfig : ProjectConfig<RevitSettings> {
    [JsonIgnore]
    public override string ProjectConfigPath { get; set; }

    [JsonIgnore]
    public override IConfigSerializer Serializer { get; set; }

    public static PluginConfig GetPluginConfig(IConfigSerializer configSerializer) {
        return new ProjectConfigBuilder()
            .SetSerializer(configSerializer)
            .SetPluginName(nameof(RevitMarkPlacement))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(PluginConfig) + ".json")
            .Build<PluginConfig>();
    }
}

internal class RevitSettings : ProjectSettings {
    public override string ProjectName { get; set; }

    public int LevelCount { get; set; } = 5;
    public double? LevelHeight { get; set; } = 3000;

    public ElementId GlobalParameterId { get; set; }

    public Selections? SelectionMode { get; set; } = RevitMarkPlacement.Models.Selections.SelectedOnViewSelection;

    public LevelHeightProvider? LevelHeightProvider { get; set; } =
        RevitMarkPlacement.Models.LevelHeightProvider.GlobalParameter;
}

internal enum Selections {
    DBSelection,
    DBViewSelection,
    SelectedOnViewSelection,
}

internal enum LevelHeightProvider {
    UserSettings,
    GlobalParameter
}

internal class SystemPluginConfig {
    public SystemPluginConfig() {
        FamilyNames = [FamilyTopName, FamilyBottomName];
        FamilyTypeNames = [FamilyTypeTopName, FamilyTypeBottomName];
    }
    
    public ICollection<string> FamilyNames { get; }
    public ICollection<string> FamilyTypeNames { get; }
    
    public int MaxLevelCount => 12;
    public int MaxLevelHeightMm => 10000; // mm

    public string FamilyTypeTopName => "Вверх";
    public string FamilyTypeBottomName => "Вниз";

    public string FamilyTopName => "ТипАн_Отметка_ТипЭт_Разрез_Вверх";
    public string FamilyBottomName => "ТипАн_Отметка_ТипЭт_Разрез_Вниз";

    public string FirstLevelParamName => "Уровень_1";
    public string FirstLevelOnParamName => "Вкл_Уровень_1";

    public string LevelHeightParamName => "Высота типового этажа";
    public string LevelCountParamName => "Количество типовых этажей";

    public string SpotDimensionIdParamName => "Id высотной отметки";

    public string FilterSpotName => "_(auto)";
}
