using Autodesk.Revit.DB;

using RevitFinishing.Services;

namespace RevitFinishing.Models.Finishing
{
    internal class FinishingWallFactory : IFinishingFactory {
        public FinishingElement Create(Element element) {
            var paramService = new ParamCalculationService();
            return new FinishingWall(element, paramService);
        }
    }
}
