using Autodesk.Revit.DB;

using RevitFinishing.Services;

namespace RevitFinishing.Models.Finishing
{
    internal class FinishingFloorFactory : IFinishingFactory {
        private readonly ParamCalculationService _paramService;

        public FinishingFloorFactory(ParamCalculationService paramService) {
            _paramService = paramService;
        }

        public FinishingElement Create(Element element) {
            return new FinishingFloor(element, _paramService);
        }
    }
}
