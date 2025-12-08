using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

using dosymep.WPF.ViewModels;

using RevitParamsChecker.Models;
using RevitParamsChecker.Models.Checks;

namespace RevitParamsChecker.ViewModels.Checks;

internal class CheckViewModel : BaseViewModel, IEquatable<CheckViewModel>, IName {
    private readonly Check _check;
    private readonly Guid _guid;
    private string _name;
    private bool _isSelected;
    private ObservableCollection<string> _selectedFiles;
    private ObservableCollection<string> _selectedFilters;
    private ObservableCollection<string> _selectedRules;
    private bool _modified;

    public CheckViewModel(Check check) {
        _check = check ?? throw new ArgumentNullException(nameof(check));
        Name = _check.Name;
        Modified = true;
        SelectedFiles = [.._check.Files];
        SelectedFilters = [.._check.Filters];
        SelectedRules = [.._check.Rules];
        _guid = Guid.NewGuid();
        PropertyChanged += OnModelPropertyChanged;
    }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public bool IsSelected {
        get => _isSelected;
        set => RaiseAndSetIfChanged(ref _isSelected, value);
    }

    public bool Modified {
        get => _modified;
        set => RaiseAndSetIfChanged(ref _modified, value);
    }

    public ObservableCollection<string> SelectedFiles {
        get => _selectedFiles;
        set => RaiseAndSetIfChanged(ref _selectedFiles, value);
    }

    public ObservableCollection<string> SelectedFilters {
        get => _selectedFilters;
        set => RaiseAndSetIfChanged(ref _selectedFilters, value);
    }

    public ObservableCollection<string> SelectedRules {
        get => _selectedRules;
        set => RaiseAndSetIfChanged(ref _selectedRules, value);
    }

    public bool Equals(CheckViewModel other) {
        if(other is null) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return _guid.Equals(other._guid);
    }

    public override bool Equals(object obj) {
        return Equals(obj as CheckViewModel);
    }

    public override int GetHashCode() {
        return _guid.GetHashCode();
    }

    public Check GetCheck() {
        _check.Name = Name;
        _check.Files = [..SelectedFiles];
        _check.Filters = [..SelectedFilters];
        _check.Rules = [..SelectedRules];
        return _check;
    }

    private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName == nameof(Name)
           || e.PropertyName == nameof(SelectedFilters)
           || e.PropertyName == nameof(SelectedFiles)
           || e.PropertyName == nameof(SelectedRules)) {
            Modified = true;
        }
    }
}
