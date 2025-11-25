using System.Collections.Generic;

using RevitBuildCoordVolumes.Models.Enums;

namespace RevitBuildCoordVolumes.Models.Settings;

internal class ConfigSettings {
    public List<string> Documents { get; set; }
    public string TypeZone { get; set; }
    public PositionProviderType UpPositionProvider { get; set; }
    public PositionProviderType BottomPositionProvider { get; set; }
    public List<ParamMap> ParamMaps { get; set; }
    public List<string> TypeSlabs { get; set; }
    public double SearchSide { get; set; }

    public void ApplyDefaultValues() {
        Documents = [];
        TypeZone = RevitConstants.TypeZone;
        UpPositionProvider = PositionProviderType.UpPosition;
        BottomPositionProvider = PositionProviderType.UpPosition;
        ParamMaps = GetDefaultParamMaps();
        SearchSide = RevitConstants.SearchSide;
    }

    private List<ParamMap> GetDefaultParamMaps() {
        return RevitConstants.GetDefaultParamMaps();
    }
}
