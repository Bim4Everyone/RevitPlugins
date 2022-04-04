namespace RevitMarkPlacement.ViewModels {
    internal interface IFloorHeightProvider {
        bool IsEnabled { get; }
        string GetFloorHeight();
    }
}
