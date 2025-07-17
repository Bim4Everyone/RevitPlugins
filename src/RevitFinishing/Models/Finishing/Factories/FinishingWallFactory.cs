using Autodesk.Revit.DB;

using RevitFinishing.Services;

namespace RevitFinishing.Models.Finishing
{
    internal class FinishingWallFactory : IFinishingFactory {
        private readonly ParamCalculationService _paramService;

        public FinishingWallFactory(ParamCalculationService paramService) {
            _paramService = paramService;
        }

        public FinishingElement Create(Element element) {
            return new FinishingWall(element, _paramService);
        }
    }
}
