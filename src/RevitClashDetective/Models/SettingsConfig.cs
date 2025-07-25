
using System.Windows.Media;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

namespace RevitClashDetective.Models;

internal class SettingsConfig : ProjectConfig {
    [JsonIgnore]
    public override string ProjectConfigPath { get; set; }

    [JsonIgnore]
    public override IConfigSerializer Serializer { get; set; }

    public ElementVisibilitySettings MainElementVisibilitySettings { get; set; } = new ElementVisibilitySettings() {
        Color = Color.FromRgb(255, 255, 0),
        Transparency = 40
    };

    public ElementVisibilitySettings SecondElementVisibilitySettings { get; set; } = new ElementVisibilitySettings() {
        Color = Color.FromRgb(0, 255, 255),
        Transparency = 40
    };


    public static SettingsConfig GetSettingsConfig(IConfigSerializer serializer) {
        return new dosymep.Bim4Everyone.ProjectConfigs.ProjectConfigBuilder()
            .SetSerializer(serializer)
            .SetPluginName(nameof(RevitClashDetective))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(SettingsConfig) + ".json")
            .Build<SettingsConfig>();
    }
}
