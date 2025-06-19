using System.Collections.Generic;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace RevitRoughFinishingDesign.Models;
internal class PluginConfig : ProjectConfig<RevitSettings> {
    [JsonIgnore] public override string ProjectConfigPath { get; set; }

    [JsonIgnore] public override IConfigSerializer Serializer { get; set; }
    public static PluginConfig GetPluginConfig() {
        return new ProjectConfigBuilder()
            .SetSerializer(new ConfigSerializer())
            .SetPluginName(nameof(RevitRoughFinishingDesign))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(PluginConfig) + ".json")
            .Build<PluginConfig>();
    }
}

internal class RevitSettings : ProjectSettings {
    public override string ProjectName { get; set; }
    public string SaveProperty { get; set; }
    public List<PairModel> PairModels { get; set; }
    public double? LineOffset { get; set; } = null; // Смещение линии для оформления
    public bool IsAutomated { get; set; } // Автоматический анализ линий по изображениям
}
