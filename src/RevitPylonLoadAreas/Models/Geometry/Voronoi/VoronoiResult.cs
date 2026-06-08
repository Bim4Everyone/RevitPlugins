using System.Collections.Generic;

namespace RevitPylonLoadAreas.Models.Geometry.Voronoi;

internal sealed class VoronoiResult {
    public VoronoiResult(IReadOnlyList<VoronoiCell> cells) {
        Cells = cells;
    }

    public IReadOnlyList<VoronoiCell> Cells { get; }
}
