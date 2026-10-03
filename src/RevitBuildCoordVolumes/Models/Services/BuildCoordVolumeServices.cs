using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;

using RevitBuildCoordVolumes.Models.Interfaces;

namespace RevitBuildCoordVolumes.Models.Services;

internal sealed class BuildCoordVolumeServices {

    public BuildCoordVolumeServices (
        ILocalizationService localizationService,
        IRevitParamFactory revitParamFactory,
        IGeomObjectFactory geomObjectFactory,
        ICategoryAvailabilityService categoryAvailabilityService,
        IParamAvailabilityService paramAvailabilityService,
        IDirectShapeObjectFactory directShapeObjectFactory,
        IParamSetter paramSetter,
        IWindowService windowService,
        ISpatialElementCheckService spatialElementCheckService,
        ISpatialElementDividerService spatialDivider,
        IColumnFactory columnFactory,
        IGeomObjectsBuildService geomObjectsBuildService) {

        LocalizationService = localizationService;
        RevitParamFactory = revitParamFactory;
        GeomObjectFactory = geomObjectFactory;
        CategoryAvailabilityService = categoryAvailabilityService;
        ParamAvailabilityService = paramAvailabilityService;
        DirectShapeObjectFactory = directShapeObjectFactory;
        ParamSetter = paramSetter;
        WindowService = windowService;
        SpatialElementCheckService = spatialElementCheckService;
        SpatialDivider = spatialDivider;
        ColumnFactory = columnFactory;
        GeomObjectsBuildService = geomObjectsBuildService;
    }

    public ISpatialElementDividerService SpatialDivider { get; }
    public IColumnFactory ColumnFactory { get; }
    public IGeomObjectFactory GeomObjectFactory { get; }
    public IParamSetter ParamSetter { get; }
    public IDirectShapeObjectFactory DirectShapeObjectFactory { get; }
    public ICategoryAvailabilityService CategoryAvailabilityService { get; }
    public IParamAvailabilityService ParamAvailabilityService { get; }
    public ILocalizationService LocalizationService { get; }
    public IRevitParamFactory RevitParamFactory { get; }
    public IWindowService WindowService { get; }
    public ISpatialElementCheckService SpatialElementCheckService { get; }
    public IGeomObjectsBuildService GeomObjectsBuildService { get; }
}
