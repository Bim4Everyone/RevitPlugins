using Autodesk.Revit.DB;

using RevitFinishing.Services;

namespace RevitFinishing.Models.Finishing
{
    internal class FinishingBaseboardFactory : IFinishingFactory {
        private readonly ParamCalculationService _paramService;

        public FinishingBaseboardFactory(ParamCalculationService paramService) {
            _paramService = paramService;
        }

        public FinishingElement Create(Element element) {
            var paramService = new ParamCalculationService();
            return new FinishingBaseboard(element, _paramService);
        }
    }
}
