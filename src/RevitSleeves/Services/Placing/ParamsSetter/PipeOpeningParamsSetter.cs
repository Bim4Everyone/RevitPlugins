using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using dosymep.SimpleServices;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.ParamsSetter;
internal class PipeOpeningParamsSetter : IParamsSetter<ClashModel<Pipe, FamilyInstance>> {
    private readonly IPlacingErrorsService _errorsService;
    private readonly ILocalizationService _localizationService;
    private readonly ClashModel<Pipe, FamilyInstance> _clash;

    public PipeOpeningParamsSetter(
        IPlacingErrorsService errorsService,
        ILocalizationService localizationService,
        ClashModel<Pipe, FamilyInstance> clash) {
        _errorsService = errorsService ?? throw new ArgumentNullException(nameof(errorsService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _clash = clash ?? throw new ArgumentNullException(nameof(clash));
    }


    public void SetParamValues(FamilyInstance sleeve) {
        // TODO
    }
}
