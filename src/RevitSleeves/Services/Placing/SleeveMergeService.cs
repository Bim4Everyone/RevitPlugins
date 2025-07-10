using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Autodesk.Revit.DB;

using RevitSleeves.Exceptions;
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

        var mergeModels = FindMergeModels(sleeves);

        var opts = _optsProvider.GetOpts([.. mergeModels.Where(m => m.Count > 1)]);
        int i = 0;
        foreach(var opt in opts) {
            ct.ThrowIfCancellationRequested();
            progress?.Report(++i);
            FamilyInstance instance = default;
            try {
                instance = _revitRepository.CreateInstance(opt.FamilySymbol, opt.Point, opt.Level);
                _revitRepository.RotateElement(instance, opt.Point, opt.Rotation);
                opt.ParamsSetter.SetParamValues(instance);
            } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                _errorsService.AddError(opt.DependentElements, "Exceptions.CannotMergeSleeves");
                if(instance is not null) {
                    _revitRepository.DeleteElement(instance.Id);
                }
                continue;
            } catch(CannotCreateSleeveException) {
                if(instance is not null) {
                    _revitRepository.DeleteElement(instance.Id);
                }
                continue;
            }
            _revitRepository.DeleteElements([.. opt.DependentElements.Select(e => e.Id)]);
        }
    }

    /// <summary>
    /// Находит гильзы для объединения, используя граф
    /// </summary>
    /// <param name="sleeves">Все исходные гильзы</param>
    /// <returns>Коллекция объединенных гильз</returns>
    private ICollection<SleeveMergeModel> FindMergeModels(ICollection<SleeveModel> sleeves) {
        var allSleeves = sleeves.ToList();
        var visited = new HashSet<SleeveModel>();
        var mergeModels = new List<SleeveMergeModel>();

        foreach(var sleeve in allSleeves) {
            if(visited.Contains(sleeve)) {
                continue;
            }

            var mergeCandidates = new List<SleeveModel>();
            var queue = new Queue<SleeveModel>();
            queue.Enqueue(sleeve);
            visited.Add(sleeve);

            while(queue.Count > 0) {
                var current = queue.Dequeue();
                mergeCandidates.Add(current);

                foreach(var neighbor in allSleeves) {
                    if(!visited.Contains(neighbor) && CanMerge(current, neighbor)) {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            var mergeGroup = new SleeveMergeModel(mergeCandidates.First());
            foreach(var s in mergeCandidates.Skip(1)) {
                mergeGroup.TryAddSleeve(s);
            }

            mergeModels.Add(mergeGroup);
        }

        return mergeModels;
    }

    private bool CanMerge(SleeveModel first, SleeveModel second) {
        return first.CanMerge(second);
    }
}
