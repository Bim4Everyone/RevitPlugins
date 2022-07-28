
using dosymep.Bim4Everyone;

using RevitClashDetective.Models.Value;

namespace RevitOpeningPlacement.Models.OpeningPlacement {

    internal abstract class ParameterValuePair {
        public string ParamName { get; set; }
        public virtual ParamValue Value { get; }
    }

    internal class ParameterValuePair<T> : ParameterValuePair where T : ParamValue {
        public T TValue { get; set; }
        public override ParamValue Value => TValue;
    }
}
