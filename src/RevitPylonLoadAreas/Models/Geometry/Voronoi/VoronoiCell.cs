namespace RevitPylonLoadAreas.Models.Geometry.Voronoi;

internal sealed class VoronoiCell {
    public VoronoiCell(Polygon2D polygon, VoronoiSite site) {
        Polygon = polygon;
        Site = site;
    }

    public Polygon2D Polygon { get; }

    public VoronoiSite Site { get; }
}
