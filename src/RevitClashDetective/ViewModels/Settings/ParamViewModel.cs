using System;
using System.Collections.Generic;

using dosymep.WPF.ViewModels;

namespace RevitClashDetective.ViewModels.Settings;
internal class ParamViewModel : BaseViewModel, IEquatable<ParamViewModel> {
    private string _name;

    public ParamViewModel() {
        _name = string.Empty;
        Guid = Guid.NewGuid();
    }


    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public Guid Guid { get; }


    public bool Equals(ParamViewModel other) {
        if(other is null) { return false; }
        return ReferenceEquals(this, other) || Guid == other.Guid;
    }

    public override int GetHashCode() {
        return -1125283371 + EqualityComparer<Guid>.Default.GetHashCode(Guid);
    }

    public override bool Equals(object obj) {
        return Equals(obj as ParamViewModel);
    }
}
