using System;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class StringParameterGetter : IParameterGetter<StringParamValue> {
        /// <summary>
        /// Название параметра в Revit
        /// </summary>
        private readonly string _paramName;


        /// <summary>
        /// Конструктор объекта, предоставляющего значение заданного параметра
        /// </summary>
        /// <param name="paramName">Название параметра Revit</param>
        /// <param name="valueGetter">Объект, предоставляющий значение параметра</param>
        public StringParameterGetter(string paramName, IValueGetter<StringParamValue> valueGetter) {
            if(valueGetter is null) {
                throw new ArgumentNullException(nameof(valueGetter));
            }
            if(string.IsNullOrWhiteSpace(paramName)) {
                throw new ArgumentException(nameof(paramName));
            }
            _paramName = paramName;
            ValueGetter = valueGetter;
        }

        public IValueGetter<StringParamValue> ValueGetter { get; set; }


        public ParameterValuePair<StringParamValue> GetParamValue() {
            return new ParameterValuePair<StringParamValue>() {
                ParamName = _paramName,
                Value = ValueGetter.GetValue()
            };
        }
    }
}
