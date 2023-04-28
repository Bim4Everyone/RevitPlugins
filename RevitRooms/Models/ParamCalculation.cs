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

#if REVIT_2020_OR_LESS

        protected static double ConvertValueToSquareMeters(double? value, int accuracy) {
            return value.HasValue ? Math.Round(UnitUtils.ConvertFromInternalUnits(value.Value, DisplayUnitType.DUT_SQUARE_METERS), accuracy, MidpointRounding.AwayFromZero) : 0;
        }

        protected static double ConvertValueToInternalUnits(double value) {
            return UnitUtils.ConvertToInternalUnits(value, DisplayUnitType.DUT_SQUARE_METERS);
        }

#else
        
        protected double ConvertValueToSquareMeters(double? value, int accuracy) {
            return value.HasValue ? Math.Round(UnitUtils.ConvertFromInternalUnits(value.Value, UnitTypeId.SquareMeters), accuracy, MidpointRounding.AwayFromZero) : 0;
        }

        protected double ConvertValueToInternalUnits(double value) {
            return UnitUtils.ConvertToInternalUnits(value, UnitTypeId.SquareMeters);
        }

#endif
    }
}
