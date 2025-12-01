using System;
using System.Collections.Generic;

using dosymep.WPF.ViewModels;

namespace RevitParamsChecker.ViewModels.Filtration;

internal class FilterViewModel : BaseViewModel, IEquatable<FilterViewModel> {
    private readonly Guid _guid;
    private string _name;

    public FilterViewModel() {
        _guid = Guid.NewGuid();
    }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public override bool Equals(object obj) {
        return Equals(obj as FilterViewModel);
    }

    public override int GetHashCode() {
        return _guid.GetHashCode();
    }

    public bool Equals(FilterViewModel other) {
        if(ReferenceEquals(other, null)) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return _guid == other._guid;
    }
}
