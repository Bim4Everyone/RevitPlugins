using System.Collections.Generic;

using RevitBuildCoordVolumes.Models.Enums;

namespace RevitBuildCoordVolumes.Models.Settings;

internal class ConfigSettings {
    public AlgorithmType AlgorithmType { get; set; }
    public string TypeZone { get; set; }
    public List<ParamMap> ParamMaps { get; set; }
    public List<string> Documents { get; set; }
    public List<string> TypeSlabs { get; set; }
    public double SquareSideMm { get; set; }

    public void ApplyDefaultValues(SystemPluginConfig systemPluginConfig) {
        AlgorithmType = AlgorithmType.AdvancedAreaExtrude;
        TypeZone = string.Empty;
        ParamMaps = systemPluginConfig.GetDefaultParamMaps();
        Documents = [];
        TypeSlabs = [];
        SquareSideMm = systemPluginConfig.SquareSide;
    }
}
