using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using dosymep.SimpleServices;

using RevitSleeves.Models;
using RevitSleeves.Models.Config;
using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Core;
using RevitSleeves.Services.Placing.PointFinder;

namespace RevitSleeves.Services.Placing.ParamsSetter;
internal class PipeFloorParamsSetter : ParamsSetter, IParamsSetter<ClashModel<Pipe, Floor>> {
    private readonly RevitRepository _revitRepository;
    private readonly IPlacingErrorsService _errorsService;
    private readonly ILocalizationService _localizationService;
    private readonly IPointFinder<ClashModel<Pipe, Floor>> _pointFinder;
    private readonly IGeometryUtils _geometryUtils;
    private readonly SleevePlacementSettingsConfig _config;
    private readonly ClashModel<Pipe, Floor> _clash;

    public PipeFloorParamsSetter(
        RevitRepository revitRepository,
        IPlacingErrorsService errorsService,
        ILocalizationService localizationService,
        IPointFinder<ClashModel<Pipe, Floor>> pointFinder,
        IGeometryUtils geometryUtils,
        SleevePlacementSettingsConfig config,
        ClashModel<Pipe, Floor> clash) {

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _errorsService = errorsService ?? throw new ArgumentNullException(nameof(errorsService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _pointFinder = pointFinder ?? throw new ArgumentNullException(nameof(pointFinder));
        _geometryUtils = geometryUtils ?? throw new ArgumentNullException(nameof(geometryUtils));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _clash = clash ?? throw new ArgumentNullException(nameof(clash));
    }


    public void SetParamValues(FamilyInstance sleeve) {
        // TODO
        SetElevation(sleeve, _pointFinder.GetPoint(_clash));
        SetInclineAngle(sleeve, Math.PI / 2);
        SetDiameter(sleeve, 1);
        double topOffset = _config.PipeSettings.Offsets[OffsetType.FromSleeveEndToTopFloorFace];
        double length = _geometryUtils.GetFloorThickness(_clash.StructureElement)
            + _revitRepository.ConvertToInternal(topOffset);
        SetLength(sleeve, length);
    }
}
