using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using RevitSleeves.Models;
using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Placing.PlacingOptsProvider;

namespace RevitSleeves.Services.Placing;
internal class SleeveMergeService : ISleeveMergeService {
    private readonly RevitRepository _revitRepository;
    private readonly IPlacingOptsProvider<SleeveMergeModel> _optsProvider;
    private readonly IPlacingErrorsService _errorsService;

    public SleeveMergeService(
        RevitRepository revitRepository,
        IPlacingOptsProvider<SleeveMergeModel> optsProvider,
        IPlacingErrorsService errorsService) {

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _optsProvider = optsProvider ?? throw new ArgumentNullException(nameof(optsProvider));
        _errorsService = errorsService ?? throw new ArgumentNullException(nameof(errorsService));
    }


    public void MergeSleeves(
        ICollection<SleeveModel> sleeves,
        IProgress<int> progress,
        CancellationToken ct) {

        var sleevesToExclude = new List<SleeveModel>();
        var mergeModels = new List<SleeveMergeModel>();

        int i = 0;
        foreach(var sleeve in sleeves) {
            ct.ThrowIfCancellationRequested();
            progress?.Report(++i);
            sleevesToExclude.Add(sleeve);
            var mergeModel = new SleeveMergeModel(sleeve);
            mergeModels.Add(mergeModel);
            var sleevesToIterate = sleeves.Except(sleevesToExclude).ToArray();
            foreach(var sleeveToMerge in sleevesToIterate) {
                bool added = mergeModel.TryAddSleeve(sleeveToMerge);
                if(added) {
                    sleevesToExclude.Add(sleeveToMerge);
                }
            }
        }

        var opts = _optsProvider.GetOpts([.. mergeModels.Where(m => m.Count > 1)]);
        foreach(var opt in opts) {
            try {
                var instance = _revitRepository.CreateInstance(opt.FamilySymbol, opt.Point, opt.Level);
                _revitRepository.RotateElement(instance, opt.Point, opt.Rotation);
                opt.ParamsSetter.SetParamValues(instance);
            } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                _errorsService.AddError(opt.DependentElements, "Exceptions.CannotMergeSleeves");
                continue;
            }
            _revitRepository.DeleteElements([.. opt.DependentElements.Select(e => e.Id)]);
        }
    }
}
