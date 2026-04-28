using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitSplitMepCurve.Models;
using RevitSplitMepCurve.Models.Enums;
using RevitSplitMepCurve.Models.Splittable;

namespace RevitSplitMepCurve.Services.Providers;

internal abstract class ElementsProvider : IElementsProvider {
    protected readonly RevitRepository _revitRepository;

    protected ElementsProvider(RevitRepository revitRepository) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
    }

    public abstract MepClass MepClass { get; }

    public ICollection<SplittableElement> GetElements(SelectionMode selectionMode) {
        ICollection<MEPCurve> raw = selectionMode switch {
            SelectionMode.SelectedElements => GetSelected(),
            SelectionMode.ActiveView => GetActiveView(),
            SelectionMode.ActiveDocument => GetActiveDocument(),
            _ => throw new ArgumentOutOfRangeException(nameof(selectionMode))
        };
        double minLength = _revitRepository.Application.ShortCurveTolerance;
        var filtered = raw.Where(c => IsLongEnough(c, minLength)).ToArray();
        var displacements = _revitRepository.GetDisplacementElements();
        return [.. filtered.Select(c => CreateSplittable(c, displacements))];
    }

    protected abstract ICollection<MEPCurve> GetSelected();

    protected abstract ICollection<MEPCurve> GetActiveView();

    protected abstract ICollection<MEPCurve> GetActiveDocument();

    protected abstract SplittableElement CreateSplittable(
        MEPCurve curve, ICollection<DisplacementElement> all);

    protected static ICollection<DisplacementElement> FilterDisplacements(
        MEPCurve curve, ICollection<DisplacementElement> all) {
        return all.Where(de => de.GetDisplacedElementIds().Contains(curve.Id)).ToArray();
    }

    private static bool IsLongEnough(MEPCurve curve, double minLength) {
        if(curve.Location is not LocationCurve locationCurve) {
            return false;
        }
        return locationCurve.Curve.Length >= minLength;
    }
}
