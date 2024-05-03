using System;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    /// <summary>
    /// Класс, предоставляющий значение параметра с типом данных integer
    /// </summary>
    internal class IntegerParameterGetter : IParameterGetter<IntParamValue> {
        private readonly string _paramName;

        /// <summary>
        /// Конструктор класса, предоставляющего значение параметра с типом данных integer
        /// </summary>
        /// <param name="paramName">Название параметра</param>
        /// <param name="valueGetter">Значение параметра</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public IntegerParameterGetter(string paramName, IValueGetter<IntParamValue> valueGetter) {
            if(paramName is null) {
                throw new ArgumentNullException(nameof(paramName));
            }
            if(string.IsNullOrWhiteSpace(paramName)) {
                throw new ArgumentException(nameof(paramName));
            }
            if(valueGetter is null) {
                throw new ArgumentNullException(nameof(valueGetter));
            }
            _paramName = paramName;
            ValueGetter = valueGetter;
        }


        public IValueGetter<IntParamValue> ValueGetter { get; set; }

        public ParameterValuePair<IntParamValue> GetParamValue() {
            return new ParameterValuePair<IntParamValue>() {
                ParamName = _paramName,
                Value = ValueGetter.GetValue()
            };
        }
    }
}
