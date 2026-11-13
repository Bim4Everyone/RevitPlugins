using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSplitMepCurve.Models;
using RevitSplitMepCurve.Models.Enums;
using RevitSplitMepCurve.Models.Splittable;

namespace RevitSplitMepCurve.Services.Providers;

internal class PipesProvider : ElementsProvider {
    public PipesProvider(RevitRepository revitRepository) : base(revitRepository) {
    }

    public override MepClass MepClass => MepClass.Pipes;

    protected override ICollection<MEPCurve> GetSelectedElements() {
        return _revitRepository.GetSelectedElements<Pipe>(BuiltInCategory.OST_PipeCurves).ToArray();
    }

    protected override ICollection<MEPCurve> GetElementsOnActiveView() {
        return _revitRepository.GetActiveViewElements<Pipe>(BuiltInCategory.OST_PipeCurves).ToArray();
    }

    protected override ICollection<MEPCurve> GetElementsInDocument() {
        return _revitRepository.GetElements<Pipe>(BuiltInCategory.OST_PipeCurves).ToArray();
    }

    protected override SplittableElement CreateSplittable(MEPCurve curve, ICollection<DisplacementElement> all) {
        var filtered = FilterDisplacements(curve, all);
        return new SplittablePipe((Pipe) curve, filtered);
    }
}
