using System;
using System.Collections.Generic;
using System.ComponentModel;

using dosymep.WPF.ViewModels;

using RevitParamsChecker.Models;
using RevitParamsChecker.Models.Filtration;

namespace RevitParamsChecker.ViewModels.Filtration;

internal class FilterViewModel : BaseViewModel, IEquatable<FilterViewModel>, IName {
    private readonly Guid _guid;
    private string _name;
    private Filter _filter;
    private bool _modified;

    public FilterViewModel(Filter filter) {
        // TODO адаптировать под либу фильтрации
        _filter = filter ?? throw new ArgumentNullException(nameof(filter));
        Name = _filter.Name;
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

    public Filter GetFilter() {
        _filter.Name = Name;
        return _filter;
    }

    private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName == nameof(Filter.Name)) {
            Modified = true;
        }
    }
}
