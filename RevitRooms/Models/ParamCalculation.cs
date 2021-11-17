using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

using RevitRooms.ViewModels;

namespace RevitRooms.Models {
    internal abstract class ParamCalculation<T> {
        public T OldValue { get; protected set; }
        public T CalculationValue { get; protected set; }

        public abstract double GetPercentChange();
        public abstract void CalculateParam(SpatialElementViewModel spatialElement);

        public Phase Phase { get; set; }
        public RevitParam RevitParam { get; set; }

#if D2020 || R2020

        protected static double ConvertValueToSquareMeters(double? value, int accuracy) {
            return value.HasValue ? Math.Round(UnitUtils.ConvertFromInternalUnits(value.Value, DisplayUnitType.DUT_SQUARE_METERS), accuracy) : 0;
        }

        protected static double ConvertValueToInternalUnits(double value) {
            return UnitUtils.ConvertToInternalUnits(value, DisplayUnitType.DUT_SQUARE_METERS);
        }

#elif D2021 || R2021 || D2022 || R2022
        
        protected double ConvertValueToSquareMeters(double? value, int accuracy) {
            return value.HasValue ? Math.Round(UnitUtils.ConvertFromInternalUnits(value.Value, UnitTypeId.SquareMeters), accuracy) : 0;
        }

        protected double ConvertValueToInternalUnits(double value) {
            return UnitUtils.ConvertToInternalUnits(value, UnitTypeId.SquareMeters);
        }

#endif
    }
}
