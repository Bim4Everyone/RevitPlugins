using System;
using System.Collections.Generic;
using System.Threading;

using Autodesk.Revit.DB;

using RevitSleeves.Models;
using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing;
internal class SleevePlacerService : ISleevePlacerService {
    private readonly RevitRepository _revitRepository;
    private readonly IPlacingErrorsService _errorsService;

    public SleevePlacerService(
        RevitRepository revitRepository,
        IPlacingErrorsService errorsService) {

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _errorsService = errorsService ?? throw new ArgumentNullException(nameof(errorsService));
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
                _errorsService.AddError(opt.DependentElements, "Exceptions.CannotCreateSleeve");
                continue;
            }
            try {
                _revitRepository.RotateElement(instance, opt.Point, opt.Rotation);
            } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                _errorsService.AddError(opt.DependentElements, "Exceptions.CannotRotateSleeve");
                continue;
            }
            try {
                opt.ParamsSetter.SetParamValues(instance);
            } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                _errorsService.AddError(opt.DependentElements, "Exceptions.CannotSetSleeveParams");
                continue;
            }
            sleeves.Add(new SleeveModel(instance));
        }
        return sleeves;
    }
}
