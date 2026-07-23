using RevitAreaBoundaries.Models.Enums;
using RevitAreaBoundaries.Settings;

namespace RevitAreaBoundaries.Models.Processors;

internal class BoundaryProcessorSelector (OutBoundaryDrawer outerDrawer){
    public IBoundaryDrawer SelectProcessor(AreaBoundarySettings areaBoundarySettings) {
        return areaBoundarySettings.BoundaryDrawer.DrawerType switch {
            DrawerType.OuterBoundary => outerDrawer,
            _ => outerDrawer
        };
    }
    
}
