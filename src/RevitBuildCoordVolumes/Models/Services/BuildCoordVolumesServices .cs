using RevitBuildCoordVolumes.Models.Interfaces;

namespace RevitBuildCoordVolumes.Models.Services;
internal sealed class BuildCoordVolumesServices {
    public ISpatialElementDividerService SpatialDivider { get; } = new SpatialElementDividerService();
    public ISlabNormalizeService SlabNormalizer { get; } = new SlabNormalizeService();
    public IColumnFactory ColumnFactory { get; } = new ColumnFactory();
    public IContourService ContourService { get; } = new ContourService();
    public IGeomElementFactory GeomElementFactory { get; }

    public BuildCoordVolumesServices() {
        GeomElementFactory = new GeomElementFactory(ContourService);
    }
}
