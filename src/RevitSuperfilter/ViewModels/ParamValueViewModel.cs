using dosymep.WPF.ViewModels;

namespace RevitSuperfilter.ViewModels;

internal sealed class ParamValueViewModel : BaseViewModel {
    public string Value { get; }

    public ParamValueViewModel(string value) {
        Value = value;
    }
}
