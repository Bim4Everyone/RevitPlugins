using System;
using System.Collections.ObjectModel;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitParamsChecker.ViewModels.Checks;

internal class CheckViewModel : BaseViewModel, IEquatable<CheckViewModel> {
    private readonly Guid _guid;
    private string _name;
    private bool _isSelected;
    private ObservableCollection<string> _selectedFiles;
    private ObservableCollection<string> _selectedFilters;
    private ObservableCollection<string> _selectedRules;

    public CheckViewModel() {
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
}
