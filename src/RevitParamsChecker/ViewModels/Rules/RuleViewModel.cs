using System;
using System.Collections.Generic;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

namespace RevitParamsChecker.ViewModels.Rules;

internal class RuleViewModel : BaseViewModel, IEquatable<RuleViewModel> {
    private readonly Guid _guid;
    private string _name;
    private string _description;

    public RuleViewModel(
        ICollection<LogicalOperatorViewModel> logicalOperators,
        ICollection<ComparisonOperatorViewModel> comparisonOperators) {
        _guid = Guid.NewGuid();
        RootSet = new ParamsSetViewModel(logicalOperators, comparisonOperators); // TODO
    }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public string Description {
        get => _description;
        set => RaiseAndSetIfChanged(ref _description, value);
    }

    public ParamsSetViewModel RootSet { get; }

    public bool Equals(RuleViewModel other) {
        if(other is null) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return _guid.Equals(other._guid);
    }

    public override bool Equals(object obj) {
        return Equals(obj as RuleViewModel);
    }

    public override int GetHashCode() {
        return _guid.GetHashCode();
    }
}
