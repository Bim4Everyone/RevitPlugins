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
            SelectionMode.SelectedElements => GetSelectedElements(),
            SelectionMode.ActiveView => GetElementsOnActiveView(),
            SelectionMode.ActiveDocument => GetElementsInDocument(),
            _ => throw new ArgumentOutOfRangeException(nameof(selectionMode))
        };

        var displacements = _revitRepository.GetDisplacementElements();
        return [.. raw.Where(IsValid).Select(c => CreateSplittable(c, displacements))];
    }

    protected abstract ICollection<MEPCurve> GetSelectedElements();

    protected abstract ICollection<MEPCurve> GetElementsOnActiveView();

    protected abstract ICollection<MEPCurve> GetElementsInDocument();

    protected abstract SplittableElement CreateSplittable(MEPCurve curve, ICollection<DisplacementElement> all);

    /// <summary>
    /// Фильтрует наборы смещения, в которых есть заданный элемент ВИС
    /// </summary>
    /// <param name="curve">Элемент ВИС</param>
    /// <param name="all">Все наборы смещения для проверки</param>
    /// <returns>Наборы смещения, в которых есть заданный элемент ВИС</returns>
    protected ICollection<DisplacementElement> FilterDisplacements(
        MEPCurve curve,
        ICollection<DisplacementElement> all) {
        return all.Where(de => de.GetDisplacedElementIds().Contains(curve.Id)).ToArray();
    }

    /// <summary>
    /// Проверяет, что элемент не короткий и не горизонтальный
    /// </summary>
    private bool IsValid(MEPCurve mepCurve) {
        double tolerance = _revitRepository.Application.ShortCurveTolerance;
        if(mepCurve.Location is not LocationCurve locationCurve) {
            return false;
        }

        if(locationCurve.Curve.Length <= tolerance) {
            return false;
        }

        if(Math.Abs(locationCurve.Curve.GetEndPoint(0).Z - locationCurve.Curve.GetEndParameter(1))
           <= tolerance) {
            return false;
        }

        return true;
    }
}
