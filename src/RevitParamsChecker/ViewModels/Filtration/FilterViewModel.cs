using System;
using System.ComponentModel;

using Bim4Everyone.RevitFiltration.Controls;

using dosymep.WPF.ViewModels;

using RevitParamsChecker.Models;

namespace RevitParamsChecker.ViewModels.Filtration;

internal class FilterViewModel : BaseViewModel, IEquatable<FilterViewModel>, IName {
    private readonly Guid _guid;
    private string _name;
    private bool _modified;

    public FilterViewModel(string name, ILogicalFilterProvider filterProvider) {
        if(string.IsNullOrWhiteSpace(name)) {
            throw new ArgumentException(nameof(name));
        }

        FilterProvider = filterProvider ?? throw new ArgumentNullException(nameof(filterProvider));
        Name = name;
        Modified = true;
        _guid = Guid.NewGuid();
        PropertyChanged += OnModelPropertyChanged;
    }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public bool Modified {
        get => _modified;
        set => RaiseAndSetIfChanged(ref _modified, value);
    }

    public ILogicalFilterProvider FilterProvider { get; }

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

    private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName == nameof(Name)) {
            Modified = true;
        }
    }
}
