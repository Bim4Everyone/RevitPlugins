using System.Collections.Generic;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

using RevitSplitMepCurve.Models.Enums;

namespace RevitSplitMepCurve.Models;

internal class PluginConfig : ProjectConfig<RevitSettings> {
    [JsonIgnore]
    public override string ProjectConfigPath { get; set; }

    [JsonIgnore]
    public override IConfigSerializer Serializer { get; set; }

    public static PluginConfig GetPluginConfig(IConfigSerializer configSerializer) {
        return new ProjectConfigBuilder()
            .SetSerializer(configSerializer)
            .SetPluginName(nameof(RevitSplitMepCurve))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(PluginConfig) + ".json")
            .Build<PluginConfig>();
    }
}

internal class RevitSettings : ProjectSettings {
    public override string ProjectName { get; set; }

    public MepClass SelectedMepClass { get; set; } = MepClass.Pipes;

    public SelectionMode SelectedMode { get; set; } = SelectionMode.ActiveView;

    /// <summary>Имя типоразмера круглого соединителя.</summary>
    public string ConnectorRoundSymbolName { get; set; } = string.Empty;

    /// <summary>Имя типоразмера прямоугольного соединителя.</summary>
    public string ConnectorRectangleSymbolName { get; set; } = string.Empty;

    /// <summary>Имена уровней, исключённых пользователем.</summary>
    public List<string> UncheckedLevelNames { get; set; } = [];

    public bool ShowSplitErrors { get; set; } = true;
}
