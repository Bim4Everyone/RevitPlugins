using System;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

using RevitRooms.ViewModels;

namespace RevitRooms.Models.Calculation {
    internal abstract class DoubleParamCalculation : ParamCalculation<double>, IParamCalculation {
        protected readonly int _percent;
        protected readonly int _accuracy;

        public DoubleParamCalculation(int percent, int accuracy) {
            _percent = percent;
            _accuracy = accuracy;
        }

        public bool SetParamValue(SpatialElementViewModel spatialElement) {
            if(CheckSetParamValue(spatialElement)) {
                bool isBigChanges = IsBigChanges(spatialElement.Element);
                SetParamValueInternal(spatialElement);

                return isBigChanges;
            }

            return false;
        }

        protected virtual void SetParamValueInternal(SpatialElementViewModel spatialElement) {
            spatialElement.Element.SetParamValue(RevitParam, ConvertValueToInternalUnits(CalculationValue));
        }

        protected virtual bool CheckSetParamValue(SpatialElementViewModel spatialElement) {
            return spatialElement.Phase?.ElementId == Phase?.Id;
        }

        protected virtual bool IsBigChanges(Element element) {
            OldValue = ConvertValueToSquareMeters((double?) element.GetParamValueOrDefault(RevitParam), _accuracy);
            return OldValue != 0 && GetPercentChange() > _percent;
        }

        public double GetDifferences() {
            return OldValue - CalculationValue;
        }

        public override double GetPercentChange() {
            return Math.Abs(OldValue - CalculationValue) / Math.Abs(OldValue) * 100;
        }
    }
}
