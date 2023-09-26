using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.RealOpeningPlacement.ValueGetters {
    /// <summary>
    /// Класс, предоставляющий значение параметра <see cref="RealOpeningPlacer.RealOpeningIsManualBimModelPart"/>
    /// </summary>
    internal class IsManualBimModelPartValueGetter : IValueGetter<IntParamValue> {
        /// <summary>
        /// Конструктор класса, предоставляющего значение параметра <see cref="RealOpeningPlacer.RealOpeningIsManualBimModelPart"/>
        /// </summary>
        public IsManualBimModelPartValueGetter() { }


        public IntParamValue GetValue() {
            return new IntParamValue(1);
        }
    }
}
