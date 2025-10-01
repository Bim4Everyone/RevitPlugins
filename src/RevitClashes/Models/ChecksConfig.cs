using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

using RevitClashDetective.Models.FilterModel;

namespace RevitClashDetective.Models;
internal class ChecksConfig : ProjectConfig {
    public string RevitVersion { get; set; }
    public List<Check> Checks { get; set; } = [];
    [JsonIgnore]
    public override string ProjectConfigPath { get; set; }
    [JsonIgnore]
    public override IConfigSerializer Serializer { get; set; }

    public static ChecksConfig GetChecksConfig(string revitFileName, Document document) {
        return document is null
            ? throw new ArgumentNullException(nameof(document))
            : new ProjectConfigBuilder()
            .SetSerializer(new RevitClashConfigSerializer(new RevitClashesSerializationBinder(), document))
            .SetPluginName(nameof(RevitClashDetective))
            .SetRelativePath(revitFileName)
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(ChecksConfig) + ".json")
            .Build<ChecksConfig>();
    }
}

internal class Check {
    public string Name { get; set; }
    public SelectionConfig FirstSelection { get; set; }
    public SelectionConfig SecondSelection { get; set; }
}

internal class SelectionConfig {
    public List<string> Filters { get; set; } = [];
    public List<string> Files { get; set; } = [];
}

internal class RevitClashDetectiveConfig : ProjectConfig {
    [JsonIgnore]
    public override string ProjectConfigPath { get; set; }
    [JsonIgnore]
    public override IConfigSerializer Serializer { get; set; }
    public string LastRunPath { get; set; }

    public static RevitClashDetectiveConfig GetRevitClashDetectiveConfig() {
        return new dosymep.Bim4Everyone.ProjectConfigs.ProjectConfigBuilder()
            .SetPluginName(nameof(RevitClashDetective))
            .SetProjectConfigName(nameof(RevitClashDetectiveConfig))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetSerializer(new ConfigSerializer())
            .Build<RevitClashDetectiveConfig>();

    }
}
