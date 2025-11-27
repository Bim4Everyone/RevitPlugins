using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

namespace RevitParamsChecker.Models.Rules;

public class RulesConfig : ProjectConfig {
    [JsonIgnore]
    public override string ProjectConfigPath { get; set; }

    [JsonIgnore]
    public override IConfigSerializer Serializer { get; set; }

    public Rule[] Rules { get; set; } = [];

    public static RulesConfig GetConfig(IConfigSerializer configSerializer) {
        return new ProjectConfigBuilder()
            .SetSerializer(configSerializer)
            .SetPluginName(nameof(RevitParamsChecker))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(RulesConfig) + ".json")
            .Build<RulesConfig>();
    }
}
