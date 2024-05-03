
using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class DoubleParameterGetter : IParameterGetter<DoubleParamValue> {
        private readonly string _paramName;

        public DoubleParameterGetter(string paramName, IValueGetter<DoubleParamValue> valueGetter) {
            _paramName = paramName;
            ValueGetter = valueGetter;
        }

        public IValueGetter<DoubleParamValue> ValueGetter { get; set; }

        public ParameterValuePair<DoubleParamValue> GetParamValue() {
            return new ParameterValuePair<DoubleParamValue>() {
                ParamName = _paramName,
                Value = ValueGetter.GetValue()
            };
        }
    }
}
