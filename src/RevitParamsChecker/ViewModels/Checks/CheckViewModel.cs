using System;
using System.Collections.ObjectModel;

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

    public CheckViewModel(Check check) {
        _check = check ?? throw new ArgumentNullException(nameof(check));
        Name = _check.Name;
        SelectedFiles = [.._check.Files];
        SelectedFilters = [.._check.Filters];
        SelectedRules = [.._check.Rules];
        _guid = Guid.NewGuid();
    }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public bool IsSelected {
        get => _isSelected;
        set => RaiseAndSetIfChanged(ref _isSelected, value);
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
}
