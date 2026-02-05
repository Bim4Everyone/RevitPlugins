namespace RevitMarkPlacement.Models;

internal interface IFloorHeightProvider {
    bool IsEnabled { get; }
    LevelHeightProvider LevelHeightProvider { get; }

    string GetErrorText(SystemPluginConfig systemPluginConfig);
    double? GetFloorHeight();
    
    void LoadConfig(RevitSettings settings);
    void SaveConfig(RevitSettings settings);
}
