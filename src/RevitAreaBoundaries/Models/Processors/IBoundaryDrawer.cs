using RevitAreaBoundaries.Models.Enums;
using RevitAreaBoundaries.Services;
using RevitAreaBoundaries.Settings;

namespace RevitAreaBoundaries.Models.Processors;

internal interface IBoundaryDrawer {
    DrawerType DrawerType { get; }
    void DrawBoundaries(AreaBoundarySettings areaBoundarySettings, ProgressService progressService);
}
