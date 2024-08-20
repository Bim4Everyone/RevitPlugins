using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal abstract class LengthConverter {
        /// <summary>
        /// Класс для конвертации длины из футов в мм и обратно
        /// </summary>
        protected LengthConverter() { }


        /// <summary>
        /// Конвертирует заданное значение длины из футов в мм
        /// </summary>
        /// <param name="feetValue">Длина в футах</param>
        /// <returns>Длина в мм</returns>
        protected double ConvertFromInternal(double feetValue) {
#if REVIT_2020_OR_LESS
            return UnitUtils.ConvertFromInternalUnits(feetValue, DisplayUnitType.DUT_MILLIMETERS);
#else
            return UnitUtils.ConvertFromInternalUnits(feetValue, UnitTypeId.Millimeters);
#endif
        }

        /// <summary>
        /// Конвертирует заданное значение длины из мм в футы
        /// </summary>
        /// <param name="mmValue">Длина в мм</param>
        /// <returns>Длина в футах</returns>
        protected double ConvertToInternal(double mmValue) {
#if REVIT_2020_OR_LESS
            return UnitUtils.ConvertToInternalUnits(mmValue, DisplayUnitType.DUT_MILLIMETERS);
#else
            return UnitUtils.ConvertToInternalUnits(mmValue, UnitTypeId.Millimeters);
#endif
        }
    }
}
