using System;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

using RevitRooms.ViewModels;

namespace RevitRooms.Models.Calculation;
internal abstract class CurrencyParamCalculation : ParamCalculation<int>, IParamCalculation {
    public virtual bool SetParamValue(SpatialElementViewModel spatialElement) {
        // Для параметра RoomsCount был изменен StorageType с int на double.
        // При этом скрипт должен корректно обрабатывать старые int-значения.
        // Для этого добавлено дополнительное условие при заполнении параметра.
        var param = spatialElement.Element.LookupParameter(RevitParam.Name);

        if(param.StorageType == StorageType.Double) {
            double value = spatialElement.Element.GetParamValueOrDefault<double>(RevitParam);
            OldValue = (int) value;
            spatialElement.Element.SetParamValue(RevitParam, (double) CalculationValue);
        } else {
            OldValue = spatialElement.Element.GetParamValueOrDefault<int>(RevitParam);
            spatialElement.Element.LookupParameter(RevitParam.Name).Set(CalculationValue);
        }

        return false;
    }

    public double GetDifferences() {
        return OldValue - CalculationValue;
    }

    public override double GetPercentChange() {
        return Math.Abs(OldValue - CalculationValue) / Math.Abs(OldValue) * 100;
    }
}
