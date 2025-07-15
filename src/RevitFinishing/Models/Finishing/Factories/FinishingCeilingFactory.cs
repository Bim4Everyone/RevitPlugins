using Autodesk.Revit.DB;

using RevitFinishing.Services;

namespace RevitFinishing.Models.Finishing;
internal class FinishingCeilingFactory : IFinishingFactory {
    public FinishingElement Create(Element element) {
        var paramService = new ParamCalculationService();
        return new FinishingCeiling(element, paramService);
    }
}
