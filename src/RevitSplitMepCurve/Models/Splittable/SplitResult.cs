using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitSplitMepCurve.Models.Splittable;

internal class SplitResult {
    public SplitResult(
        MEPCurve original,
        ICollection<MEPCurve> newSegments,
        ICollection<FamilyInstance> insertedConnectors,
        ICollection<DisplacementElement> displacementElements) {
        Original = original ?? throw new ArgumentNullException(nameof(original));
        NewSegments = newSegments ?? [];
        InsertedConnectors = insertedConnectors ?? [];
        DisplacementElements = displacementElements ?? [];
    }

    public MEPCurve Original { get; }

    public ICollection<MEPCurve> NewSegments { get; }

    public ICollection<FamilyInstance> InsertedConnectors { get; }

    public ICollection<DisplacementElement> DisplacementElements { get; }

    /// <summary>
    /// Обновляет рабочий набор у новых сегментов и коннекторов,
    /// добавляет их во все DisplacementElement, в которые входил Original.
    /// </summary>
    public void UpdateSegments() {
        var sourceWorksetId = Original.WorksetId;
        Element[] newEls = [..NewSegments, .. InsertedConnectors];

        foreach(var element in newEls) {
            SetWorksetId(element, sourceWorksetId);
        }

        foreach(var de in DisplacementElements) {
            UpdateDisplacementElement(de, newEls);
        }
    }

    private void SetWorksetId(Element element, WorksetId worksetId) {
        if(!element.Document.IsWorkshared) {
            return;
        }

        element.SetParamValue(BuiltInParameter.ELEM_PARTITION_PARAM, worksetId.IntegerValue);
    }

    private void UpdateDisplacementElement(DisplacementElement de, ICollection<Element> newEls) {
        var view = (View) de.Document.GetElement(de.OwnerViewId);
        HashSet<ElementId> ids = [
            ..de.GetDisplacedElementIds(),
            ..newEls.Where(el => DisplacementElement.IsAllowedAsDisplacedElement(el)
                                 && !DisplacementElement.IsElementDisplacedInView(view, el.Id))
                .Select(el => el.Id)
        ];

        de.SetDisplacedElementIds(ids);
    }
}
