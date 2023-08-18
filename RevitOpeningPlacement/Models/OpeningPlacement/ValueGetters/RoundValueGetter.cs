using System;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    /// <summary>
    /// Абстрактный класс для работы с округлением единиц длины Revit
    /// </summary>
    internal abstract class RoundValueGetter {
        /// <summary>
        /// Число знаков после запятой для округления
        /// </summary>
        private const int _digitsRound = 5;

        protected RoundValueGetter() { }


        /// <summary>
        /// Округляет полученное значение длины (в футах) до заданного количества миллиметров по правилам округления математики и возвращает округленную длину в футах.
        /// Если значение округления 0 или меньше, будет произведено округление по умолчанию до 1 мм.
        /// </summary>
        /// <param name="ftValue">Размер в футах, который нужно округлить до заданного количества миллиметров</param>
        /// <param name="mmRound">Значение округления в миллиметрах</param>
        /// <returns>Размер в футах, округленный до заданного количества миллиметров</returns>
        protected double RoundFeetToMillimeters(double ftValue, int mmRound) {
            var ftRound = mmRound > 0 ? GetFeetRound(mmRound) : GetFeetRound(1);
            return Math.Round(ftValue / ftRound, MidpointRounding.AwayFromZero) * ftRound;
        }

        /// <summary>
        /// Округляет полученное значение длины (в футах) до ближайшего сверху целого заданного количества миллиметров и возвращает округленную длину в футах.
        /// Если значение округления 0 или меньше, будет произведено округление по умолчанию до 1 мм.
        /// </summary>
        /// <param name="ftValue">Размер в футах, который нужно округлить до заданного количества миллиметров</param>
        /// <param name="mmRound">Значение округления в миллиметрах</param>
        /// <returns>Размер в футах, округленный до заданного количества миллиметров</returns>
        protected double RoundToCeilingFeetToMillimeters(double ftValue, int mmRound) {
            var ftRound = mmRound > 0 ? GetFeetRound(mmRound) : GetFeetRound(1);
            return Math.Ceiling(Math.Round(ftValue / ftRound, _digitsRound, MidpointRounding.AwayFromZero)) * ftRound;
        }

        /// <summary>
        /// Округляет полученное значение длины (в футах) до ближайшего снизу целого заданного количества миллиметров и возвращает округленную длину в футах.
        /// Если значение округления 0 или меньше, будет произведено округление по умолчанию до 1 мм.
        /// </summary>
        /// <param name="ftValue">Размер в футах, который нужно округлить до заданного количества миллиметров</param>
        /// <param name="mmRound">Значение округления в миллиметрах</param>
        /// <returns>Размер в футах, округленный до заданного количества миллиметров</returns>
        protected double RoundToFloorFeetToMillimeters(double ftValue, int mmRound) {
            var ftRound = mmRound > 0 ? GetFeetRound(mmRound) : GetFeetRound(1);
            return Math.Floor(Math.Round(ftValue / ftRound, _digitsRound, MidpointRounding.AwayFromZero)) * ftRound;
        }


#if REVIT_2020_OR_LESS
        /// <summary>
        /// Значение округления координат в футах (единицах длины в Revit), равное конвертированному <see cref="MmRound"/>
        /// </summary>
        private double GetFeetRound(int mmRound) {
            return UnitUtils.ConvertToInternalUnits(mmRound, DisplayUnitType.DUT_MILLIMETERS);
        }
#else
        /// <summary>
        /// Значение округления координат в футах (единицах длины в Revit), равное конвертированному <see cref="MmRound"/>
        /// </summary>
        private double GetFeetRound(int mmRound) {
            return UnitUtils.ConvertToInternalUnits(mmRound, UnitTypeId.Millimeters);
        }
#endif
    }
}
