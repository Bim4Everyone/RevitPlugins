
using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.OpeningPlacement;

namespace RevitOpeningPlacement.Models.Interfaces {
    internal interface IParameterGetter<T> where T : ParamValue {
        IValueGetter<T> ValueGetter { get; set; }
        ParameterValuePair<T> GetParamValue();
    }
}
