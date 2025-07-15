using Autodesk.Revit.DB;

using RevitFinishing.Services;

namespace RevitFinishing.Models.Finishing
{
    internal class FinishingBaseboardFactory : IFinishingFactory {
        public FinishingElement Create(Element element) {
            var paramService = new ParamCalculationService();
            return new FinishingBaseboard(element, paramService);
        }
    }
}
