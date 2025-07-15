using Autodesk.Revit.DB;

using RevitFinishing.Services;

namespace RevitFinishing.Models.Finishing;
internal class FinishingCeilingFactory : IFinishingFactory {
    private readonly ParamCalculationService _paramService;

    public FinishingCeilingFactory(ParamCalculationService paramService) {
        _paramService = paramService;
    }

    public FinishingElement Create(Element element) {
        var paramService = new ParamCalculationService();
        return new FinishingCeiling(element, _paramService);
    }
}
