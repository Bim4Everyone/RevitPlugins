namespace RevitMarkPlacement.Models;

internal interface IFloorHeightProvider {
    bool IsEnabled { get; }
    LevelHeightProvider LevelHeightProvider { get; }
    
    double? GetFloorHeight();
    
    void LoadConfig(RevitSettings settings);
    void SaveConfig(RevitSettings settings);
}
