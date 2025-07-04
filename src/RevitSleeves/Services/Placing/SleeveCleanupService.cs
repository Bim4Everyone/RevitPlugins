using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;

using dosymep.Revit;

using RevitSleeves.Models;
using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing;
internal class SleeveCleanupService : ISleeveCleanupService {
    private readonly List<ElementId> _duplicatedSleeves;
    private readonly RevitRepository _revitRepository;

    public SleeveCleanupService(RevitRepository revitRepository) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _duplicatedSleeves = [];
    }


    public ICollection<SleeveModel> CleanupSleeves(
        ICollection<SleeveModel> oldSleeves,
        ICollection<SleeveModel> newSleeves,
        IProgress<int> progress,
        CancellationToken ct) {

        // TODO добавить удаление гильз, которые находятся полностью внутри уже существующих
        var sleevesToDelete = newSleeves.Where(s => _duplicatedSleeves.Contains(s.GetFamilyInstance().Id)).ToArray();
        var cleanedSleeves = newSleeves.Except(sleevesToDelete).ToArray();
        int i = 0;
        foreach(var sleeve in sleevesToDelete) {
            ct.ThrowIfCancellationRequested();
            progress?.Report(++i);
            _revitRepository.DeleteElement(sleeve.GetFamilyInstance().Id);
        }
        return cleanedSleeves;
    }

    public void FailureProcessor(object sender, FailuresProcessingEventArgs e) {
        var fas = e.GetFailuresAccessor();

        var fmas = fas.GetFailureMessages().ToList();

        foreach(var fma in fmas) {
            var definition = fma.GetFailureDefinitionId();
            if(definition == BuiltInFailures.OverlapFailures.DuplicateInstances) {
                // в дубликаты заносить все элементы, кроме самого старого, у которого значение Id наименьшее
                var ids = fma.GetFailingElementIds().OrderBy(elId => elId.GetIdValue()).Skip(1);
                foreach(var id in ids) {
                    _duplicatedSleeves.Add(id);
                }
            }
        }
    }
}
