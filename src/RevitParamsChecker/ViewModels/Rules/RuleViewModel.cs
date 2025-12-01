using System;

using dosymep.WPF.ViewModels;

namespace RevitParamsChecker.ViewModels.Rules;

internal class RuleViewModel : BaseViewModel, IEquatable<RuleViewModel> {
    private readonly Guid _guid;
    private string _name;

    public RuleViewModel() {
        _guid = Guid.NewGuid();
    }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

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
