using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.WPF.ViewModels;

using RevitLintelsManager.Models;

namespace RevitLintelsManager.ViewModels;

internal class LintelConfigRuleViewModel : BaseViewModel {
    private string _name;
    private LintelPlaceRuleViewModel _selectedRule;

    public ObservableCollection<LintelPlaceRuleViewModel> PlaceRules { get; } = [];
    public bool IsReadOnly { get; set; }
    public bool IsDirty { get; set; }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public LintelPlaceRuleViewModel SelectedRule {
        get => _selectedRule;
        set {
            RaiseAndSetIfChanged(ref _selectedRule, value);
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public LintelConfigRule GetConfig() {
        return new LintelConfigRule {
            RuleConfigName = Name,
            LintelPlaceRules = PlaceRules.Select(rule => rule.GetRule()).ToArray()
        };
    }
}
