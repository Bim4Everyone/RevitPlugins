using System;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Exceptions;
using RevitSleeves.Models;
using RevitSleeves.Models.Config;
using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Core;
using RevitSleeves.Services.Placing.PointFinder;

namespace RevitSleeves.Services.Placing.ParamsSetter;
internal class PipeFloorParamsSetter : PipeParamsSetter, IParamsSetter<ClashModel<Pipe, Floor>> {
    private readonly IPointFinder<ClashModel<Pipe, Floor>> _pointFinder;
    private readonly IGeometryUtils _geometryUtils;
    private readonly ClashModel<Pipe, Floor> _clash;

    public PipeFloorParamsSetter(
        RevitRepository revitRepository,
        IErrorsService errorsService,
        IPointFinder<ClashModel<Pipe, Floor>> pointFinder,
        IGeometryUtils geometryUtils,
        SleevePlacementSettingsConfig config,
        ClashModel<Pipe, Floor> clash) : base(revitRepository, errorsService, config) {

        _pointFinder = pointFinder ?? throw new ArgumentNullException(nameof(pointFinder));
        _geometryUtils = geometryUtils ?? throw new ArgumentNullException(nameof(geometryUtils));
        _clash = clash ?? throw new ArgumentNullException(nameof(clash));
    }


    public void SetParamValues(FamilyInstance sleeve) {
        SetElevation(sleeve, _pointFinder.GetPoint(_clash));
        SetInclineAngle(sleeve, Math.PI / 2);
        var diameterRange = GetSleeveDiameterRange(_clash.MepElement);
        if(diameterRange is null) {
            _errorsService.AddError([_clash.MepElement], "Exceptions.CannotFindDiameterRange");
            throw new CannotCreateSleeveException();
        }
        SetDiameter(sleeve, GetSleeveDiameter(diameterRange));
        SetThickness(sleeve, GetSleeveThickness(diameterRange));
        double topOffset = _config.PipeSettings.Offsets
            .First(o => o.OffsetType == OffsetType.FromSleeveEndToTopFloorFace).Value;
        double length = _geometryUtils.GetFloorThickness(_clash.StructureElement)
            + _revitRepository.ConvertToInternal(topOffset);
        SetLength(sleeve, length);
        SetStringParameters(sleeve, _clash.MepElement);
    }
}
