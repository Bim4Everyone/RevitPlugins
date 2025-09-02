using System;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Core;
using RevitSleeves.Services.Placing.PointFinder;

namespace RevitSleeves.Services.Placing.ParamsSetter;
internal class MergeModelParamsSetter : ParamsSetter, IParamsSetter<SleeveMergeModel> {
    private readonly IErrorsService _errorsService;
    private readonly ILocalizationService _localizationService;
    private readonly IPointFinder<SleeveMergeModel> _pointFinder;
    private readonly SleeveMergeModel _sleeveModel;

    public MergeModelParamsSetter(
        IErrorsService errorsService,
        ILocalizationService localizationService,
        IPointFinder<SleeveMergeModel> pointFinder,
        SleeveMergeModel sleeveModel) {

        _errorsService = errorsService ?? throw new ArgumentNullException(nameof(errorsService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _pointFinder = pointFinder ?? throw new ArgumentNullException(nameof(pointFinder));
        _sleeveModel = sleeveModel ?? throw new ArgumentNullException(nameof(sleeveModel));
    }


    public void SetParamValues(FamilyInstance sleeve) {
        SetElevation(sleeve, _pointFinder.GetPoint(_sleeveModel));
        var orientation = _sleeveModel.GetOrientation();
        double angle = Math.Asin(orientation.Z);
        SetInclineAngle(sleeve, angle);
        SetDiameter(sleeve, _sleeveModel.GetDiameter());
        (var start, var end) = _sleeveModel.GetEndPoints();
        double length = (end - start).GetLength();
        SetLength(sleeve, length);
    }
}
