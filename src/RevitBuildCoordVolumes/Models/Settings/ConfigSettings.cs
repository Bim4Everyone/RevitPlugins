using System.Collections.Generic;

using RevitBuildCoordVolumes.Models.Enums;

namespace RevitBuildCoordVolumes.Models.Settings;

internal class ConfigSettings {
    public AlgorithmType AlgorithmType { get; set; }
    public BuilderMode BuilderMode { get; set; }
    public string TypeZone { get; set; }
    public List<ParamMap> ParamMaps { get; set; }
    public List<string> Documents { get; set; }
    public List<string> TypeSlabs { get; set; }
    public double SquareSideMm { get; set; }
    public double SquareAngleDeg { get; set; }

    public void ApplyDefaultValues(SystemPluginConfig systemPluginConfig) {
        AlgorithmType = AlgorithmType.SlabBasedAlgorithm;
        BuilderMode = BuilderMode.AutomaticBuilder;
        TypeZone = string.Empty;
        ParamMaps = systemPluginConfig.GetAdvancedParamMaps();
        Documents = [];
        TypeSlabs = [];
        SquareSideMm = systemPluginConfig.SquareSideMm;
        SquareAngleDeg = systemPluginConfig.SquareAngleDeg;
    }
}
