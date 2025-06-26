using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using dosymep.SimpleServices;

using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Placing.PointFinder;

namespace RevitSleeves.Services.Placing.ParamsSetter;
internal class PipeFloorParamsSetter : ParamsSetter, IParamsSetter<ClashModel<Pipe, Floor>> {
    private readonly IPlacingErrorsService _errorsService;
    private readonly ILocalizationService _localizationService;
    private readonly IPointFinder<ClashModel<Pipe, Floor>> _pointFinder;
    private readonly ClashModel<Pipe, Floor> _clash;

    public PipeFloorParamsSetter(
        IPlacingErrorsService errorsService,
        ILocalizationService localizationService,
        IPointFinder<ClashModel<Pipe, Floor>> pointFinder,
        ClashModel<Pipe, Floor> clash) {

        _errorsService = errorsService ?? throw new ArgumentNullException(nameof(errorsService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _pointFinder = pointFinder ?? throw new ArgumentNullException(nameof(pointFinder));
        _clash = clash ?? throw new ArgumentNullException(nameof(clash));
    }


    public void SetParamValues(FamilyInstance sleeve) {
        // TODO
        SetElevation(sleeve, _pointFinder.GetPoint(_clash));
        SetInclineAngle(sleeve, Math.PI / 2);
    }
}
