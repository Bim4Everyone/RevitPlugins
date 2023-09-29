using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.ValueGetters {
    /// <summary>
    /// Класс, предоставляющий значение параметра <see cref="RealOpeningArPlacer.RealOpeningIsManualBimModelPart"/>
    /// </summary>
    internal class IsManualBimModelPartValueGetter : IValueGetter<IntParamValue> {
        /// <summary>
        /// Конструктор класса, предоставляющего значение параметра <see cref="RealOpeningArPlacer.RealOpeningIsManualBimModelPart"/>
        /// </summary>
        public IsManualBimModelPartValueGetter() { }


        public IntParamValue GetValue() {
            return new IntParamValue(1);
        }
    }
}
