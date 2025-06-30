using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitSleeves.Models;
using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Placing.PointFinder;

namespace RevitSleeves.Services.Placing.ParamsSetter;
internal class PipeWallParamsSetter : ParamsSetter, IParamsSetter<ClashModel<Pipe, Wall>> {
    private readonly RevitRepository _revitRepository;
    private readonly IPlacingErrorsService _errorsService;
    private readonly ILocalizationService _localizationService;
    private readonly IPointFinder<ClashModel<Pipe, Wall>> _pointFinder;
    private readonly ClashModel<Pipe, Wall> _clash;

    public PipeWallParamsSetter(
        RevitRepository revitRepository,
        IPlacingErrorsService errorsService,
        ILocalizationService localizationService,
        IPointFinder<ClashModel<Pipe, Wall>> pointFinder,
        ClashModel<Pipe, Wall> clash) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _errorsService = errorsService ?? throw new ArgumentNullException(nameof(errorsService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _pointFinder = pointFinder ?? throw new ArgumentNullException(nameof(pointFinder));
        _clash = clash ?? throw new ArgumentNullException(nameof(clash));
    }


    public void SetParamValues(FamilyInstance sleeve) {
        // TODO
        SetElevation(sleeve, _pointFinder.GetPoint(_clash));
        SetInclineAngle(sleeve, _clash.MepElement.GetParamValue<double>(BuiltInParameter.RBS_PIPE_SLOPE));
        double diameter = 1;
        SetDiameter(sleeve, diameter);
        var wallNormal = _clash.StructureElement.Orientation;
        var pipeDir = ((Line) ((LocationCurve) _clash.MepElement.Location).Curve).Direction;
        double angle = wallNormal.AngleTo(pipeDir);
        angle = angle <= _revitRepository.Application.AngleTolerance ? 0 : angle;
        double length = Math.Abs(_clash.StructureElement.Width / Math.Cos(angle));
        length += angle > 0 ? Math.Abs(diameter * Math.Tan(angle)) : 0;
        SetLength(sleeve, length);
    }
}
