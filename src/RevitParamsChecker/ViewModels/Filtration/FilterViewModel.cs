using System;
using System.Collections.Generic;

using dosymep.WPF.ViewModels;

namespace RevitParamsChecker.ViewModels.Filtration;

internal class FilterViewModel : BaseViewModel, IEquatable<FilterViewModel> {
    private string _name;
    private readonly Guid _guid;

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
        return 412060723 + EqualityComparer<Guid>.Default.GetHashCode(_guid);
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

    public override string ToString() {
        return Name;
    }
}
