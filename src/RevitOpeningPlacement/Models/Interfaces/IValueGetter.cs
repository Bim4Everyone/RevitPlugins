using RevitClashDetective.Models.Value;

namespace RevitOpeningPlacement.Models.Interfaces {
    internal interface IValueGetter<T> where T : ParamValue {
        T GetValue();
    }
}
