using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    /// <summary>
    /// Класс, предоставляющий значение параметра <see cref="RevitRepository.OpeningIsManuallyPlaced"/>
    /// </summary>
    internal class IsManuallyPlacedValueGetter : IValueGetter<IntParamValue> {
        /// <summary>
        /// Конструктор класса, предоставляющего значение параметра <see cref="RevitRepository.OpeningIsManuallyPlaced"/>
        /// </summary>
        public IsManuallyPlacedValueGetter() { }


        public IntParamValue GetValue() {
            return new IntParamValue(0);
        }
    }
}
