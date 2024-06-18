using System;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class LevelOffsetValueGetter : IValueGetter<DoubleParamValue> {
        private readonly ILevelFinder _levelFinder;

        /// <summary>
        /// Класс, предоставляющий значение отметки уровня, на котором размещается отверстие, в единицах Revit
        /// </summary>
        /// <param name="levelFinder"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public LevelOffsetValueGetter(ILevelFinder levelFinder) {
            _levelFinder = levelFinder ?? throw new ArgumentNullException(nameof(levelFinder));
        }

        public DoubleParamValue GetValue() {
            return new DoubleParamValue(_levelFinder.GetLevel().Elevation);
        }
    }
}
