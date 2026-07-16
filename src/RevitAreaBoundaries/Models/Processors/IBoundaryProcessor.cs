using RevitAreaBoundaries.Services;
using RevitAreaBoundaries.Settings;

namespace RevitAreaBoundaries.Models.Processors;

internal interface IBoundaryProcessor {
    void DrawBoundaries(AreaBoundarySettings areaBoundarySettings);
}
