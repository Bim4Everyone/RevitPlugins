using RevitAreaBoundaries.Services;
using RevitAreaBoundaries.Settings;

namespace RevitAreaBoundaries.Models.Processors;

public interface IBoundaryProcessor {
    void DrawBoundaries(AreaBoundarySettings areaBoundarySettings);
}
