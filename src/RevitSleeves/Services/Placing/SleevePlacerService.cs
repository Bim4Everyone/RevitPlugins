using System;
using System.Collections.Generic;
using System.Threading;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitSleeves.Models;
using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing;
internal class SleevePlacerService : ISleevePlacerService {
    private readonly RevitRepository _revitRepository;
    private readonly IPlacingErrorsService _errorsService;
    private readonly ILocalizationService _localizationService;

    public SleevePlacerService(
        RevitRepository revitRepository,
        IPlacingErrorsService errorsService,
        ILocalizationService localizationService) {

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _errorsService = errorsService ?? throw new ArgumentNullException(nameof(errorsService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
    }


    public ICollection<SleeveModel> PlaceSleeves(
        ICollection<SleevePlacingOpts> opts,
        IProgress<int> progress,
        CancellationToken ct) {

        var sleeves = new List<SleeveModel>();
        foreach(var opt in opts) {
            FamilyInstance instance;
            try {
                instance = _revitRepository.CreateInstance(opt.FamilySymbol, opt.Point, opt.Level);
            } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                _errorsService.AddError(new ErrorModel(
                    opt.DependentElements,
                    _localizationService.GetLocalizedString("Exceptions.CannotCreateSleeve")));
                continue;
            }
            try {
                _revitRepository.RotateElement(instance, opt.Point, opt.Rotation);
            } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                _errorsService.AddError(new ErrorModel(
                    opt.DependentElements,
                    _localizationService.GetLocalizedString("Exceptions.CannotRotateSleeve")));
                continue;
            }
            try {
                opt.ParamsSetter.SetParamValues(instance);
            } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                _errorsService.AddError(new ErrorModel(
                    opt.DependentElements,
                    _localizationService.GetLocalizedString("Exceptions.CannotSetSleeveParams")));
                continue;
            }
            sleeves.Add(new SleeveModel(instance));
        }
        return sleeves;
    }
}
