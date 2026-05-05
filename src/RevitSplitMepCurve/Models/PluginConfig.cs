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
    public const MepClass DefaultMepClass = MepClass.Pipes;
    public const SelectionMode DefaultSelectionMode = SelectionMode.ActiveView;
    
    public override string ProjectName { get; set; }

    public MepClass SelectedMepClass { get; set; } = DefaultMepClass;

    public SelectionMode SelectedMode { get; set; } = DefaultSelectionMode;

    public ConnectorConfig RoundConnector { get; set; } = new ConnectorConfig();

    public ConnectorConfig RectangleConnector { get; set; } = new ConnectorConfig();

    /// <summary>Имена уровней, исключённых пользователем.</summary>
    public List<string> UncheckedLevelNames { get; set; } = [];

    public bool ShowSplitErrors { get; set; } = true;
}
