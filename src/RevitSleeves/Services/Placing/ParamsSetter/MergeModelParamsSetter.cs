using System;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.ParamsSetter;
internal class MergeModelParamsSetter : IParamsSetter<SleeveMergeModel> {
    private readonly IPlacingErrorsService _errorsService;
    private readonly ILocalizationService _localizationService;
    private readonly SleeveMergeModel _sleeveModel;

    public MergeModelParamsSetter(
        IPlacingErrorsService errorsService,
        ILocalizationService localizationService,
        SleeveMergeModel sleeveModel) {

        _errorsService = errorsService ?? throw new ArgumentNullException(nameof(errorsService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _sleeveModel = sleeveModel ?? throw new ArgumentNullException(nameof(sleeveModel));
    }


    public void SetParamValues(FamilyInstance sleeve) {
        // TODO
    }
}
