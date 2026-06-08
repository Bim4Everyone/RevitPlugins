using Autodesk.Revit.DB;

namespace RevitPylonLoadAreas.Models.Geometry.Voronoi;

internal sealed class VoronoiSite {
    public VoronoiSite(XY point, ElementId elementId) {
        Point = point;
        ElementId = elementId;
    }

    public XY Point { get; }

    public ElementId ElementId { get; }
}
