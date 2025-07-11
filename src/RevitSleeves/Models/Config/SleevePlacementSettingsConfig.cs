using System;
using System.IO;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

namespace RevitSleeves.Models.Config;

internal class SleevePlacementSettingsConfig : ProjectConfig {
    [JsonIgnore]
    public override string ProjectConfigPath { get; set; }

    [JsonIgnore]
    public override IConfigSerializer Serializer { get; set; }

    public PipeSettings PipeSettings { get; set; } = new PipeSettings();

    public bool ShowPlacingErrors { get; set; }

    public string Name { get; set; } = "default";

    [JsonIgnore]
    public const BuiltInCategory SleeveCategory = BuiltInCategory.OST_PipeFitting;

    public static SleevePlacementSettingsConfig GetPluginConfig(IConfigSerializer configSerializer) {
        try {
            return GetOverriddenBuilder(configSerializer)
                .Build<SleevePlacementSettingsConfig>();
        } catch(JsonException) {
            try {
                return GetDefaultBuilder(configSerializer)
                    .Build<SleevePlacementSettingsConfig>();
            } catch(JsonException) {
                return new SleevePlacementSettingsConfig() {
                    ProjectConfigPath = GetDefaultPath(),
                    Serializer = configSerializer
                };
            }
        }
    }

    private static ProjectConfigBuilder GetOverriddenBuilder(IConfigSerializer configSerializer) {
        var builder = GetDefaultBuilder(configSerializer);
        string configPath = SleevePlacementSettingsConfigPath.GetPluginConfig(configSerializer).Path;
        if(!string.IsNullOrWhiteSpace(configPath) && File.Exists(configPath)) {
            builder.SetProjectConfigPath(configPath);
        }
        return builder;
    }

    private static ProjectConfigBuilder GetDefaultBuilder(IConfigSerializer configSerializer) {
        return new ProjectConfigBuilder()
             .SetSerializer(configSerializer)
             .SetPluginName(nameof(RevitSleeves))
             .SetRevitVersion(ModuleEnvironment.RevitVersion)
             .SetProjectConfigName(nameof(SleevePlacementSettingsConfig) + ".json");
    }

    private static string GetDefaultPath() {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Personal),
            "dosymep",
            ModuleEnvironment.RevitVersion,
            nameof(RevitSleeves),
            nameof(SleevePlacementSettingsConfig) + ".json");
    }
}
