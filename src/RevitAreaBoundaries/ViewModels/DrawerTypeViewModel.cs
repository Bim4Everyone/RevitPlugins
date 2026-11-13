using RevitAreaBoundaries.Models.Enums;
using RevitAreaBoundaries.Models.Processors;

namespace RevitAreaBoundaries.ViewModels;

internal class DrawerTypeViewModel {
    // public AlgorithmType AlgorithmType { get; set; }
    
    public IBoundaryDrawer BoundaryDrawer { get; set; }
    
    public string Name { get; set; }
}
