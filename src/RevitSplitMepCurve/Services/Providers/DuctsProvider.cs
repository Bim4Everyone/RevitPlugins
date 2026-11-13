using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;

using RevitSplitMepCurve.Models;
using RevitSplitMepCurve.Models.Enums;
using RevitSplitMepCurve.Models.Splittable;

namespace RevitSplitMepCurve.Services.Providers;

internal class DuctsProvider : ElementsProvider {
    public DuctsProvider(RevitRepository revitRepository) : base(revitRepository) {
    }

    public override MepClass MepClass => MepClass.Ducts;

    protected override ICollection<MEPCurve> GetSelectedElements() {
        return _revitRepository.GetSelectedElements<Duct>(BuiltInCategory.OST_DuctCurves)
            .Where(IsRectangleOrRound)
            .ToArray();
    }

    protected override ICollection<MEPCurve> GetElementsOnActiveView() {
        return _revitRepository.GetActiveViewElements<Duct>(BuiltInCategory.OST_DuctCurves)
            .Where(IsRectangleOrRound)
            .ToArray();
    }

    protected override ICollection<MEPCurve> GetElementsInDocument() {
        return _revitRepository.GetElements<Duct>(BuiltInCategory.OST_DuctCurves)
            .Where(IsRectangleOrRound)
            .ToArray();
    }

    protected override SplittableElement CreateSplittable(MEPCurve curve, ICollection<DisplacementElement> all) {
        var filtered = FilterDisplacements(curve, all);
        return new SplittableDuct((Duct) curve, filtered);
    }

    private bool IsRectangleOrRound(Duct duct) {
        return duct.DuctType?.Shape is ConnectorProfileType.Round or ConnectorProfileType.Rectangular;
    }
}
