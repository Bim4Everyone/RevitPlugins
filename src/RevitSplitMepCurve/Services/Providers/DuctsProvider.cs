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

    protected override ICollection<MEPCurve> GetSelected() =>
        _revitRepository.GetSelectedElements<Duct>(BuiltInCategory.OST_DuctCurves).Cast<MEPCurve>().ToArray();

    protected override ICollection<MEPCurve> GetActiveView() =>
        _revitRepository.GetActiveViewElements<Duct>(BuiltInCategory.OST_DuctCurves).Cast<MEPCurve>().ToArray();

    protected override ICollection<MEPCurve> GetActiveDocument() =>
        _revitRepository.GetElements<Duct>(BuiltInCategory.OST_DuctCurves).Cast<MEPCurve>().ToArray();

    protected override SplittableElement CreateSplittable(MEPCurve curve, ICollection<DisplacementElement> all) {
        var filtered = FilterDisplacements(curve, all);
        return new SplittableDuct((Duct)curve, filtered);
    }
}
