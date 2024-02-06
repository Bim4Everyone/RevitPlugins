using RevitClashDetective.Models.Value;

namespace RevitOpeningPlacement.Models.OpeningPlacement {

    internal abstract class ParameterValuePair {
        public string ParamName { get; set; }
        public ParamValue Value { get; set; }
    }

    internal class ParameterValuePair<T> : ParameterValuePair where T : ParamValue {
        public new T Value {
            get => (T) base.Value;
            set => base.Value = value;
        }
    }
}
