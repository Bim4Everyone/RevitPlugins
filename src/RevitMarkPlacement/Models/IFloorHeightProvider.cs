namespace RevitMarkPlacement.Models;

internal interface IFloorHeightProvider {
    bool IsEnabled { get; }
    LevelHeightProvider LevelHeightProvider { get; }
    
    double? GetFloorHeight();
    
    void LoadConfig(AnnotationsSettings settings);
    void SaveConfig(AnnotationsSettings settings);
}
