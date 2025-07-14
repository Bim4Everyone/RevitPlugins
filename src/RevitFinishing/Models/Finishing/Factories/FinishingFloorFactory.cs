using Autodesk.Revit.DB;

using RevitFinishing.Services;

namespace RevitFinishing.Models.Finishing
{
    internal class FinishingFloorFactory : FinishingFactory {
        public override FinishingElement Create(Element element) {
            var paramService = new ParamCalculationService();
            return new FinishingFloor(element, paramService);
        }
    }
}
