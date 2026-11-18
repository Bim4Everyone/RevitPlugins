using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

using dosymep.WPF.ViewModels;

using RevitLintelsManager.Models;
using RevitLintelsManager.Services;

namespace RevitLintelsManager.ViewModels;

internal class LintelPlaceRuleViewModel : BaseViewModel {
    public event Action Changed;

    private string _name;
    private ObservableCollection<WallTypeViewModel> _filteredWalls;
    private string _searchText;
    private string _errorText;

    public LintelPlaceRuleViewModel(string name, IEnumerable<LintelRule> rules,
        IEnumerable<WallTypeViewModel> walls, SettingsCommandFactory commandFactory) {
        Name = name;
        Rules = new ObservableCollection<LintelRuleViewModel>(rules.Select(rule =>
            new LintelRuleViewModel(rule ?? new LintelRule())));
        Walls = new ObservableCollection<WallTypeViewModel>(walls);
        SearchWallsCommand = commandFactory.Create(ApplySearchWalls);
        ApplySearchWalls();
        foreach(var rule in Rules) {
            rule.PropertyChanged += OnItemChanged;
        }
        Rules.CollectionChanged += (_, e) => {
            if(e.OldItems is not null) {
                foreach(LintelRuleViewModel rule in e.OldItems) {
                    rule.PropertyChanged -= OnItemChanged;
                }
            }
            if(e.NewItems is not null) {
                foreach(LintelRuleViewModel rule in e.NewItems) {
                    rule.PropertyChanged += OnItemChanged;
                }
            }
            Changed?.Invoke();
        };
        foreach(var wall in Walls) {
            wall.PropertyChanged += OnItemChanged;
        }
        Walls.CollectionChanged += (_, e) => {
            if(e.OldItems is not null) {
                foreach(WallTypeViewModel wall in e.OldItems) {
                    wall.PropertyChanged -= OnItemChanged;
                }
            }
            if(e.NewItems is not null) {
                foreach(WallTypeViewModel wall in e.NewItems) {
                    wall.PropertyChanged += OnItemChanged;
                }
            }
            ApplySearchWalls();
            Changed?.Invoke();
        };
    }

    public ObservableCollection<LintelRuleViewModel> Rules { get; }
    public ObservableCollection<WallTypeViewModel> Walls { get; }
    public ICommand SearchWallsCommand { get; }

    public ObservableCollection<WallTypeViewModel> FilteredWalls {
        get => _filteredWalls;
        private set => RaiseAndSetIfChanged(ref _filteredWalls, value);
    }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public string SearchText {
        get => _searchText;
        set => RaiseAndSetIfChanged(ref _searchText, value);
    }

    public string ErrorText {
        get => _errorText;
        set {
            RaiseAndSetIfChanged(ref _errorText, value);
            RaisePropertyChanged(nameof(HasErrors));
        }
    }

    public bool HasErrors => !string.IsNullOrEmpty(ErrorText);

    public void ApplySearchWalls() {
        FilteredWalls = new ObservableCollection<WallTypeViewModel>(Walls.Where(wall => wall.IsAvailable)
            .Where(wall => string.IsNullOrEmpty(SearchText)
                || wall.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0));
    }

    public LintelPlaceRule GetRule() {
        return new LintelPlaceRule {
            LintelPlaceRuleName = Name,
            LintelRules = Rules.Select(row => {
                row.TryGetRule(out var rule);
                return rule;
            }).ToArray()
        };
    }

    private void OnItemChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName is nameof(LintelRuleViewModel.ErrorText) or nameof(LintelRuleViewModel.HasErrors)
            or nameof(WallTypeViewModel.IsAvailable)) {
            return;
        }
        Changed?.Invoke();
    }
}
