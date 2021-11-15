using dosymep.Bim4Everyone;

using RevitRooms.ViewModels;

namespace RevitRooms.Models.Calculation {
    internal abstract class IntParamCalculation : ParamCalculation<int>, IParamCalculation {
        public virtual bool SetParamValue(SpatialElementViewModel spatialElement) {
            spatialElement.Element.SetParamValue(RevitParam, CalculationValue);
            return false;
        }
    }
}
