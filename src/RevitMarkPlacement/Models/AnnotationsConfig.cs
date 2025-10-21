using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace RevitMarkPlacement.Models;

internal class AnnotationsConfig : ProjectConfig<AnnotationsSettings> {
    [JsonIgnore]
    public override string ProjectConfigPath { get; set; }

    [JsonIgnore]
    public override IConfigSerializer Serializer { get; set; }

    public static AnnotationsConfig GetAnnotationsConfig() {
        return new ProjectConfigBuilder()
            .SetSerializer(new ConfigSerializer())
            .SetPluginName(nameof(RevitMarkPlacement))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(AnnotationsConfig) + ".json")
            .Build<AnnotationsConfig>();
    }
}

internal class AnnotationsSettings : ProjectSettings {
    public override string ProjectName { get; set; }
   
    public int LevelCount { get; set; } = 5;
    public double? LevelHeight { get; set; } = 3000;
    
    public ElementId GlobalParameterId { get; set; }
    
    public Selections? SelectionMode { get; set; } = RevitMarkPlacement.Models.Selections.SelectedOnViewSelection;
    public LevelHeightProvider? LevelHeightProvider { get; set; } = RevitMarkPlacement.Models.LevelHeightProvider.GlobalParameter;
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
