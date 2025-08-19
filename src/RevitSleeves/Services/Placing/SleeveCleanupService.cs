using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;

using dosymep.Revit;
using dosymep.Revit.Geometry;

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

        var duplicatedSleeves = newSleeves.Where(s => _duplicatedSleeves.Contains(s.GetFamilyInstance().Id)).ToArray();
        var notDuplicatedSleeves = newSleeves.Except(duplicatedSleeves).ToArray();
        int i = 0;
        foreach(var sleeve in duplicatedSleeves) {
            ct.ThrowIfCancellationRequested();
            progress?.Report(++i);
            _revitRepository.DeleteElement(sleeve.Id);
        }
        var cleanedSleeves = new List<SleeveModel>();
        foreach(var sleeve in notDuplicatedSleeves) {
            ct.ThrowIfCancellationRequested();
            progress?.Report(++i);
            if(SleeveIsInsideOneOfOthers(sleeve, oldSleeves)) {
                _revitRepository.DeleteElement(sleeve.Id);
            } else {
                cleanedSleeves.Add(sleeve);
            }
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

    private bool SleeveIsInsideOneOfOthers(SleeveModel newSleeve, ICollection<SleeveModel> oldSleeves) {
        if(oldSleeves.Count == 0) {
            return false;
        }
        var newSleeveBbox = newSleeve.GetFamilyInstance().GetBoundingBox();
        var intersectedSleeves = new FilteredElementCollector(
            _revitRepository.Document, [.. oldSleeves.Select(s => s.Id)])
            .WherePasses(new BoundingBoxContainsPointFilter(newSleeve.Location))
            .ToElements();
        if(intersectedSleeves.Count == 0) {
            return false;
        }
        var newSleeveSolid = GetSolid(newSleeve.GetFamilyInstance());
        foreach(var intersectedSleeve in intersectedSleeves) {
            try {
                var intersectedSleeveSolid = GetSolid(intersectedSleeve);
                var intersectedSolid = BooleanOperationsUtils.ExecuteBooleanOperation(
                    intersectedSleeveSolid, newSleeveSolid, BooleanOperationsType.Intersect);
                double intersectionVolume = intersectedSolid.Volume;
                double newSleeveVolume = newSleeveSolid.Volume;
                if(Math.Abs(intersectionVolume - newSleeveVolume)
                    <= Math.Min(intersectionVolume, newSleeveVolume) * 0.01) {
                    return true;
                }
            } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                return true;
            }
        }
        return false;
    }

    private Solid GetSolid(Element element) {
        return element.GetSolids().OrderByDescending(s => s.Volume).First();
    }
}
