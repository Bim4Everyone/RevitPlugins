using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using dosymep.SimpleServices;

using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Placing.PointFinder;

namespace RevitSleeves.Services.Placing.ParamsSetter;
internal class PipeOpeningParamsSetter : ParamsSetter, IParamsSetter<ClashModel<Pipe, FamilyInstance>> {
    private readonly IPlacingErrorsService _errorsService;
    private readonly ILocalizationService _localizationService;
    private readonly IPointFinder<ClashModel<Pipe, FamilyInstance>> _pointFinder;
    private readonly ClashModel<Pipe, FamilyInstance> _clash;

    public PipeOpeningParamsSetter(
        IPlacingErrorsService errorsService,
        ILocalizationService localizationService,
        IPointFinder<ClashModel<Pipe, FamilyInstance>> pointFinder,
        ClashModel<Pipe, FamilyInstance> clash) {
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
