using System.Collections.Generic;

using RevitBuildCoordVolumes.Models.Enums;

namespace RevitBuildCoordVolumes.Models.Settings;

internal class ConfigSettings {
    public FileProviderType FileProviderType { get; set; }
    public string TypeZone { get; set; }
    public List<string> TypeSlabs { get; set; }
    public PositionProviderType UpPosition { get; set; }
    public PositionProviderType BottomPosition { get; set; }
    public double SearchSide { get; set; }
    public List<ParamMap> ParamMaps { get; set; }

    public void ApplyDefaultValues() {
        FileProviderType = FileProviderType.LinkFile;
        TypeZone = RevitConstants.TypeZone;
        TypeSlabs = GetDefaultSlabTypeNames();
        UpPosition = PositionProviderType.UpPositionProvider;
        BottomPosition = PositionProviderType.UpPositionProvider;
        SearchSide = RevitConstants.SearchSide;
        ParamMaps = GetDefaultParamMaps();
    }

    private List<ParamMap> GetDefaultParamMaps() {
        return RevitConstants.GetDefaultParamMaps();
    }

    private List<string> GetDefaultSlabTypeNames() {
        return RevitConstants.GetDefaultSlabTypeNames();
    }
}
