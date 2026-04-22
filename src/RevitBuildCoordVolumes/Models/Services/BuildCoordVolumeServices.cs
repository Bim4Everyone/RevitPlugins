using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;

using RevitBuildCoordVolumes.Models.Interfaces;

namespace RevitBuildCoordVolumes.Models.Services;

internal sealed class BuildCoordVolumeServices {

    public BuildCoordVolumeServices(
        SystemPluginConfig systemPluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        IRevitParamFactory revitParamFactory,
        IWindowService windowService) {
        LocalizationService = localizationService;
        RevitParamFactory = revitParamFactory;
        ContourService = new ContourService();
        GeomObjectFactory = new GeomObjectFactory(ContourService, revitRepository);
        CategoryAvailabilityService = new CategoryAvailabilityService(revitRepository.Document);
        DirectShapeObjectFactory = new DirectShapeObjectFactory(systemPluginConfig);
        ParamSetter = new ParamSetter(systemPluginConfig);
        WindowService = windowService;
        SpatialElementCheckService = new SpatialElementCheckService(ContourService);
        SpatialDivider = new SpatialElementDividerService(ContourService);
        SlabNormalizer = new SlabNormalizeService(systemPluginConfig);
        ColumnFactory = new ColumnFactory();
        GeomObjectConnector = new GeomObjectConnector(revitRepository, systemPluginConfig);
        GeomObjectsBuildService = new GeomObjectsBuildService(GeomObjectFactory);
    }

    public ISpatialElementDividerService SpatialDivider { get; }
    public ISlabNormalizeService SlabNormalizer { get; }
    public IColumnFactory ColumnFactory { get; }
    public IContourService ContourService { get; }
    public IGeomObjectFactory GeomObjectFactory { get; }
    public IParamSetter ParamSetter { get; }
    public IDirectShapeObjectFactory DirectShapeObjectFactory { get; }
    public ICategoryAvailabilityService CategoryAvailabilityService { get; }
    public IParamAvailabilityService ParamAvailabilityService { get; } = new ParamAvailabilityService();
    public ILocalizationService LocalizationService { get; }
    public IRevitParamFactory RevitParamFactory { get; }
    public IWindowService WindowService { get; }
    public ISpatialElementCheckService SpatialElementCheckService { get; }
    public IGeomObjectConnector GeomObjectConnector { get; }
    public IGeomObjectsBuildService GeomObjectsBuildService { get; }
}
