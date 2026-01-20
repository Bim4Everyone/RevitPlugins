using RevitBuildCoordVolumes.Models.Interfaces;

namespace RevitBuildCoordVolumes.Models.Services;
internal sealed class BuildCoordVolumesServices {

    public BuildCoordVolumesServices(RevitRepository revitRepository, SystemPluginConfig systemPluginConfig) {
        GeomElementFactory = new GeomObjectFactory(ContourService);
        CategoryAvailabilityService = new CategoryAvailabilityService(revitRepository.Document);
        DirectShapeObjectFactory = new DirectShapeObjectFactory(systemPluginConfig);
        ParamSetter = new ParamSetter(systemPluginConfig);
    }

    public ISpatialElementDividerService SpatialDivider { get; } = new SpatialElementDividerService();
    public ISlabNormalizeService SlabNormalizer { get; } = new SlabNormalizeService();
    public IColumnFactory ColumnFactory { get; } = new ColumnFactory();
    public IContourService ContourService { get; } = new ContourService();
    public IGeomObjectFactory GeomElementFactory { get; }
    public IParamSetter ParamSetter { get; }
    public IDirectShapeObjectFactory DirectShapeObjectFactory { get; }
    public ICategoryAvailabilityService CategoryAvailabilityService { get; }
    public IParamAvailabilityService ParamAvailabilityService { get; } = new ParamAvailabilityService();
}
