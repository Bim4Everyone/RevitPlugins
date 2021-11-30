using System;

using dosymep.Bim4Everyone;

using RevitRooms.ViewModels;

namespace RevitRooms.Models.Calculation {
    internal abstract class IntParamCalculation : ParamCalculation<int>, IParamCalculation {
        public virtual bool SetParamValue(SpatialElementViewModel spatialElement) {
            OldValue = (int?) spatialElement.Element.GetParamValueOrDefault(RevitParam) ?? 0;

            spatialElement.Element.SetParamValue(RevitParam, CalculationValue);
            return false;
        }

        public double GetDifferences() {
            return OldValue - CalculationValue;
        }

        public override double GetPercentChange() {
            return Math.Abs(OldValue - CalculationValue) / Math.Abs(OldValue) * 100;
        }
    }
}
