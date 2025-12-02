using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitParamsChecker.Models.Rules;
using RevitParamsChecker.Models.Rules.ComparisonOperators;
using RevitParamsChecker.Models.Rules.LogicalOperators;
using RevitParamsChecker.Services;

namespace RevitParamsChecker.ViewModels.Rules;

internal class RulesPageViewModel : BaseViewModel {
    private readonly ILocalizationService _localization;
    private readonly RulesRepository _rulesRepo;
    private readonly NameEditorService _nameEditorService;
    private readonly ICollection<LogicalOperatorViewModel> _availableLogicalOperators;
    private readonly ICollection<ComparisonOperatorViewModel> _availableComparisonOperators;
    private RuleViewModel _selectedRule;

    public RulesPageViewModel(
        ILocalizationService localization,
        RulesRepository rulesRepo,
        NameEditorService nameEditorService) {
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _rulesRepo = rulesRepo ?? throw new ArgumentNullException(nameof(rulesRepo));
        _nameEditorService = nameEditorService ?? throw new ArgumentNullException(nameof(nameEditorService));

        _availableLogicalOperators = GetAvailableLogicalOperators(_localization);
        _availableComparisonOperators = GetAvailableComparisonOperators(_localization);

        Rules = []; // TODO
        AddRuleCommand = RelayCommand.Create(AddRule);
        RenameRuleCommand = RelayCommand.Create<RuleViewModel>(RenameRule, CanRenameRule);
        RemoveRulesCommand = RelayCommand.Create<IList>(RemoveRules, CanRemoveRules);
    }

    public ICommand LoadCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand SaveAsCommand { get; }
    public ICommand AddRuleCommand { get; }
    public ICommand RenameRuleCommand { get; }
    public ICommand CopyRuleCommand { get; }
    public ICommand RemoveRulesCommand { get; }

    public ObservableCollection<RuleViewModel> Rules { get; }

    public RuleViewModel SelectedRule {
        get => _selectedRule;
        set => RaiseAndSetIfChanged(ref _selectedRule, value);
    }

    private void AddRule() {
        try {
            var newName = _nameEditorService.CreateNewName(
                _localization.GetLocalizedString("RulesPage.NewRulePrompt"),
                Rules.Select(f => f.Name).ToArray());
            Rules.Add(new RuleViewModel(_availableLogicalOperators, _availableComparisonOperators) { Name = newName });
        } catch(OperationCanceledException) {
        }
        // TODO
    }

    private void RenameRule(RuleViewModel rule) {
        try {
            rule.Name = _nameEditorService.CreateNewName(
                _localization.GetLocalizedString("RulesPage.RenameRulePrompt"),
                Rules.Select(f => f.Name).ToArray(),
                rule.Name);
        } catch(OperationCanceledException) {
        }
        // TODO
    }

    private bool CanRenameRule(RuleViewModel rule) {
        return rule is not null;
    }

    private void RemoveRules(IList items) {
        var rules = items.OfType<RuleViewModel>().ToArray();
        foreach(var rule in rules) {
            Rules.Remove(rule);
        }
        // TODO
    }

    private bool CanRemoveRules(IList items) {
        return items != null
               && items.OfType<RuleViewModel>().Count() != 0;
    }

    private ICollection<ComparisonOperatorViewModel>
        GetAvailableComparisonOperators(ILocalizationService localization) {
        return [
            new ComparisonOperatorViewModel(localization, new EqualsOperator()),
            new ComparisonOperatorViewModel(localization, new NotEqualsOperator()),
            new ComparisonOperatorViewModel(localization, new GreaterOperator()),
            new ComparisonOperatorViewModel(localization, new GreaterOrEqualOperator()),
            new ComparisonOperatorViewModel(localization, new LessOperator()),
            new ComparisonOperatorViewModel(localization, new LessOrEqualOperator()),
            new ComparisonOperatorViewModel(localization, new BeginsWithOperator()),
            new ComparisonOperatorViewModel(localization, new NotBeginsWithOperator()),
            new ComparisonOperatorViewModel(localization, new EndsWithOperator()),
            new ComparisonOperatorViewModel(localization, new NotEndsWithOperator()),
            new ComparisonOperatorViewModel(localization, new ContainsOperator()),
            new ComparisonOperatorViewModel(localization, new NotContainsOperator()),
            new ComparisonOperatorViewModel(localization, new HasValueOperator()),
            new ComparisonOperatorViewModel(localization, new HasNoValueOperator())
        ];
    }

    private ICollection<LogicalOperatorViewModel> GetAvailableLogicalOperators(ILocalizationService localization) {
        return [
            new LogicalOperatorViewModel(localization, new AndOperator()),
            new LogicalOperatorViewModel(localization, new OrOperator())
        ];
    }
}
