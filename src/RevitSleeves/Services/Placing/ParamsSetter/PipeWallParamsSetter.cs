using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using dosymep.SimpleServices;

using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Placing.PointFinder;

namespace RevitSleeves.Services.Placing.ParamsSetter;
internal class PipeWallParamsSetter : ParamsSetter, IParamsSetter<ClashModel<Pipe, Wall>> {
    private readonly IPlacingErrorsService _errorsService;
    private readonly ILocalizationService _localizationService;
    private readonly IPointFinder<ClashModel<Pipe, Wall>> _pointFinder;
    private readonly ClashModel<Pipe, Wall> _clash;

    public PipeWallParamsSetter(
        IPlacingErrorsService errorsService,
        ILocalizationService localizationService,
        IPointFinder<ClashModel<Pipe, Wall>> pointFinder,
        ClashModel<Pipe, Wall> clash) {

        _errorsService = errorsService ?? throw new ArgumentNullException(nameof(errorsService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _pointFinder = pointFinder ?? throw new ArgumentNullException(nameof(pointFinder));
        _clash = clash ?? throw new ArgumentNullException(nameof(clash));
    }


    public void SetParamValues(FamilyInstance sleeve) {
        // TODO
        SetElevation(sleeve, _pointFinder.GetPoint(_clash));
    }
}
