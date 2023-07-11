using System;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class CenterOffsetValueGetter : IValueGetter<DoubleParamValue> {
        private readonly IPointFinder _pointFinder;

        /// <summary>
        /// Конструктор класса, предоставляющего значение высотной отметки центра отверстия, кроме прямоугольных отверстий в стенах и кроме отверстий в перекрытиях
        /// Для прямоугольных отверстий в стенах использовать <see cref="CenterOffsetOfRectangleOpeningInWallValueGetter"/>
        /// </summary>
        /// <param name="pointFinder">Объект, предоставляющий координату центра отверстия</param>
        /// <exception cref="ArgumentNullException">Исключение, если <paramref name="pointFinder"/> null</exception>
        public CenterOffsetValueGetter(IPointFinder pointFinder) {
            if(pointFinder is null) {
                throw new ArgumentNullException(nameof(pointFinder));
            }
            _pointFinder = pointFinder;
        }


        public DoubleParamValue GetValue() {
            var offsetInFeet = _pointFinder.GetPoint().Z;
            return new DoubleParamValue(GetOffsetInMm(offsetInFeet));
        }

        private double GetOffsetInMm(double offsetInFeet) {
            double offsetInMm = 0;
#if REVIT_2020_OR_LESS
            offsetInMm = UnitUtils.ConvertFromInternalUnits(offsetInFeet, DisplayUnitType.DUT_MILLIMETERS);
#else
            offsetInMm = UnitUtils.ConvertFromInternalUnits(offsetInFeet, UnitTypeId.Millimeters);
#endif
            return offsetInMm;
        }
    }
}
