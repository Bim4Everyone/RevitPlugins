using dosymep.WPF.ViewModels;

using RevitParamsChecker.ViewModels.Rules;

namespace RevitParamsChecker.ViewModels.Results;

internal class RuleResultViewModel : BaseViewModel {
    public RuleResultViewModel() {
    }

    public string Name { get; }

    public ParamsSetViewModel RootParamsSet { get; }
}
