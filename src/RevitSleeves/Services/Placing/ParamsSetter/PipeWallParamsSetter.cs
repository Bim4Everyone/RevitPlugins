using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using dosymep.Revit;

using RevitSleeves.Exceptions;
using RevitSleeves.Models;
using RevitSleeves.Models.Config;
using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Core;
using RevitSleeves.Services.Placing.PointFinder;

namespace RevitSleeves.Services.Placing.ParamsSetter;
internal class PipeWallParamsSetter : PipeParamsSetter, IParamsSetter<ClashModel<Pipe, Wall>> {
    private readonly IPointFinder<ClashModel<Pipe, Wall>> _pointFinder;
    private readonly ClashModel<Pipe, Wall> _clash;

    public PipeWallParamsSetter(
        RevitRepository revitRepository,
        IErrorsService errorsService,
        IPointFinder<ClashModel<Pipe, Wall>> pointFinder,
        SleevePlacementSettingsConfig config,
        ClashModel<Pipe, Wall> clash) : base(revitRepository, errorsService, config) {

        _pointFinder = pointFinder ?? throw new ArgumentNullException(nameof(pointFinder));
        _clash = clash ?? throw new ArgumentNullException(nameof(clash));
    }


    public void SetParamValues(FamilyInstance sleeve) {
        SetElevation(sleeve, _pointFinder.GetPoint(_clash));
        SetInclineAngle(sleeve, _clash.MepElement.GetParamValue<double>(BuiltInParameter.RBS_PIPE_SLOPE));
        var diameterRange = GetSleeveDiameterRange(_clash.MepElement);
        if(diameterRange is null) {
            _errorsService.AddError([_clash.MepElement], "Exceptions.CannotFindDiameterRange");
            throw new CannotCreateSleeveException();
        }
        double diameter = GetSleeveDiameter(diameterRange);
        SetDiameter(sleeve, diameter);
        SetThickness(sleeve, GetSleeveThickness(diameterRange));
        double length = GetLength(diameter);
        SetLength(sleeve, length);
        SetStringParameters(sleeve, _clash.MepElement);
    }

    private double GetLength(double sleeveDiameter) {
        var wallNormal = _clash.StructureElement.Orientation;
        var pipeDir = ((Line) ((LocationCurve) _clash.MepElement.Location).Curve).Direction;
        double angle = wallNormal.AngleTo(pipeDir);
        angle = angle <= _revitRepository.Application.AngleTolerance ? 0 : angle;
        double length = Math.Abs(_clash.StructureElement.Width / Math.Cos(angle));
        length += angle > 0 ? Math.Abs(sleeveDiameter * Math.Tan(angle)) : 0;
        return length;
    }
}
