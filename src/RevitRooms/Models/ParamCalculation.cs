using System;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

using RevitRooms.ViewModels;

namespace RevitRooms.Models;
internal abstract class ParamCalculation<T> {
    public T OldValue { get; protected set; }
    public T CalculationValue { get; protected set; }

    public abstract double GetPercentChange();
    public abstract void CalculateParam(SpatialElementViewModel spatialElement);

    public Phase Phase { get; set; }
    public RevitParam RevitParam { get; set; }

#if REVIT_2020_OR_LESS

    protected static double ConvertValueToSquareMeters(double? value, int accuracy) {
double step = Math.Pow(10, -accuracy); // Зависимость от знаков после запятой
        double eps  = step / 1000.0; // Погрешность
        return value.HasValue 
            ? Math.Round(
                UnitUtils.ConvertFromInternalUnits(value.Value, DisplayUnitType.DUT_SQUARE_METERS) + eps,
                accuracy, 
                MidpointRounding.AwayFromZero) 
            : 0;
    }

    protected static double ConvertValueToInternalUnits(double value) {
        return UnitUtils.ConvertToInternalUnits(value, DisplayUnitType.DUT_SQUARE_METERS);
    }

#else

    protected double ConvertValueToSquareMeters(double? value, int accuracy) {
        double step = Math.Pow(10, -accuracy); // Зависимость от знаков после запятой
        double eps  = step / 1000.0; // Погрешность
        return value.HasValue 
            ? Math.Round(
                UnitUtils.ConvertFromInternalUnits(value.Value, UnitTypeId.SquareMeters) + eps, 
                accuracy, 
                MidpointRounding.AwayFromZero) 
            : 0;
    }

    protected double ConvertValueToInternalUnits(double value) {
        return UnitUtils.ConvertToInternalUnits(value, UnitTypeId.SquareMeters);
    }

#endif
}
