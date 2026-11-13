using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

using dosymep.WPF.ViewModels;

using RevitParamsChecker.Models;
using RevitParamsChecker.Models.Checks;

namespace RevitParamsChecker.ViewModels.Checks;

internal class CheckViewModel : BaseViewModel, IEquatable<CheckViewModel>, IName {
    private readonly Check _check;
    private readonly Guid _guid;
    private string _name;
    private string _warningFiles;
    private string _warningFilters;
    private string _warningRules;
    private bool _isSelected;
    private ObservableCollection<string> _selectedFiles;
    private ObservableCollection<string> _selectedFilters;
    private ObservableCollection<string> _selectedRules;
    private bool _modified;
    private EngineViewModel _selectedEngine;

    public CheckViewModel(Check check, IReadOnlyCollection<EngineViewModel> availableEngines) {
        _check = check ?? throw new ArgumentNullException(nameof(check));
        if(availableEngines is null) {
            throw new ArgumentNullException(nameof(availableEngines));
        }

        if(availableEngines.Count == 0) {
            throw new System.ArgumentOutOfRangeException(nameof(availableEngines));
        }

        Name = _check.Name;
        SelectedEngine = availableEngines.FirstOrDefault(e => e.TargetType == _check.TargetType)
            ?? availableEngines.First();
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

    public EngineViewModel SelectedEngine {
        get => _selectedEngine;
        set => RaiseAndSetIfChanged(ref _selectedEngine, value);
    }

    public string WarningFiles {
        get => _warningFiles;
        set => RaiseAndSetIfChanged(ref _warningFiles, value);
    }

    public string WarningFilters {
        get => _warningFilters;
        set => RaiseAndSetIfChanged(ref _warningFilters, value);
    }

    public string WarningRules {
        get => _warningRules;
        set => RaiseAndSetIfChanged(ref _warningRules, value);
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
        _check.TargetType = SelectedEngine.TargetType;
        _check.Files = [..SelectedFiles];
        _check.Filters = [..SelectedFilters];
        _check.Rules = [..SelectedRules];
        return _check;
    }

    private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName == nameof(Name)
           || e.PropertyName == nameof(SelectedFilters)
           || e.PropertyName == nameof(SelectedFiles)
           || e.PropertyName == nameof(SelectedRules)
           || e.PropertyName == nameof(SelectedEngine)) {
            Modified = true;
        }
    }
}
