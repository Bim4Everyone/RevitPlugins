using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class BottomOffsetFromLevelValueGetter : LengthConverter, IValueGetter<DoubleParamValue> {
        /// <summary>
        /// Провайдер отметки низа отверстия в мм
        /// </summary>
        private readonly IValueGetter<DoubleParamValue> _bottomOffsetValueGetter;
        private readonly ILevelFinder _levelFinder;

        /// <summary>
        /// Класс, предоставляющий значение отметки низа отверстия от уровня в единицах Revit
        /// </summary>
        /// <param name="bottomOffsetValueGetter">Провайдер отметки низа отверстия в мм от начала проекта</param>
        /// <param name="levelFinder">Провайдер уровня, на котором размещается отверстие</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public BottomOffsetFromLevelValueGetter(
            IValueGetter<DoubleParamValue> bottomOffsetValueGetter,
            ILevelFinder levelFinder) {

            _bottomOffsetValueGetter = bottomOffsetValueGetter
                ?? throw new System.ArgumentNullException(nameof(bottomOffsetValueGetter));
            _levelFinder = levelFinder
                ?? throw new System.ArgumentNullException(nameof(levelFinder));
        }


        public DoubleParamValue GetValue() {
            var bottomOffset = ConvertToInternal(_bottomOffsetValueGetter.GetValue().TValue);
            var levelOffset = _levelFinder.GetLevel().Elevation;
            return new DoubleParamValue(bottomOffset - levelOffset);
        }
    }
}
