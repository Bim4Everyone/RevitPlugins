using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitSplitMepCurve.Models.Splittable;

internal class SplitResult {
    public SplitResult(
        MEPCurve original,
        IList<MEPCurve> newSegments,
        IList<FamilyInstance> insertedConnectors,
        ICollection<DisplacementElement> displacementElements) {
        Original = original ?? throw new ArgumentNullException(nameof(original));
        NewSegments = newSegments ?? Array.Empty<MEPCurve>();
        InsertedConnectors = insertedConnectors ?? Array.Empty<FamilyInstance>();
        DisplacementElements = displacementElements ?? Array.Empty<DisplacementElement>();
    }

    public MEPCurve Original { get; }

    public IList<MEPCurve> NewSegments { get; }

    public IList<FamilyInstance> InsertedConnectors { get; }

    public ICollection<DisplacementElement> DisplacementElements { get; }

    /// <summary>
    /// Обновляет рабочий набор у новых сегментов и коннекторов,
    /// добавляет их во все DisplacementElement, в которые входил Original.
    /// </summary>
    public void UpdateSegments() {
        var doc = Original.Document;
        var sourceWorksetId = Original.WorksetId;

        var allNew = NewSegments.Cast<Element>()
            .Concat(InsertedConnectors.Cast<Element>())
            .ToArray();

        foreach(var element in allNew) {
            SetWorksetId(doc, element, sourceWorksetId);
        }

        if(DisplacementElements.Count == 0) {
            return;
        }

        var newIds = allNew.Select(e => e.Id).ToList();

        foreach(var de in DisplacementElements) {
            UpdateDisplacementElement(doc, de, newIds);
        }
    }

    private static void SetWorksetId(Document doc, Element element, WorksetId worksetId) {
        if(!doc.IsWorkshared) {
            return;
        }
        try {
            element.SetParamValue(BuiltInParameter.ELEM_PARTITION_PARAM, worksetId.IntegerValue);
        } catch {
            // Non-fatal
        }
    }

    private static void UpdateDisplacementElement(
        Document doc, DisplacementElement de, IList<ElementId> newIds) {
        try {
            var existingIds = de.GetDisplacedElementIds().ToList();
            var mergedIds = existingIds.Concat(newIds).Distinct().ToList();
            de.SetDisplacedElementIds(mergedIds);
        } catch {
            try {
                var displacement = de.GetRelativeDisplacement();
                var view = doc.GetElement(de.OwnerViewId) as View;
                if(view is null) {
                    return;
                }
                var existingIds = de.GetDisplacedElementIds().ToList();
                var mergedIds = existingIds.Concat(newIds).Distinct().ToList();
                DisplacementElement.Create(doc, mergedIds, displacement, view, null);
                doc.Delete(de.Id);
            } catch {
                // Non-fatal
            }
        }
    }
}
