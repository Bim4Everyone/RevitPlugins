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
    private RuleViewModel _selectedRule;

    public RulesPageViewModel(
        ILocalizationService localization,
        RulesRepository rulesRepo,
        NameEditorService nameEditorService) {
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _rulesRepo = rulesRepo ?? throw new ArgumentNullException(nameof(rulesRepo));
        _nameEditorService = nameEditorService ?? throw new ArgumentNullException(nameof(nameEditorService));

        Rules = [
            .._rulesRepo.GetRules()
                .Select(r => new RuleViewModel(r, _localization))
        ];
        AddRuleCommand = RelayCommand.Create(AddRule);
        RenameRuleCommand = RelayCommand.Create<RuleViewModel>(RenameRule, CanRenameRule);
        RemoveRulesCommand = RelayCommand.Create<IList>(RemoveRules, CanRemoveRules);
        CopyRuleCommand = RelayCommand.Create<RuleViewModel>(CopyRule, CanCopyRule);
        LoadCommand = RelayCommand.Create(Load);
        SaveCommand = RelayCommand.Create(Save, CanSave);
        SaveAsCommand = RelayCommand.Create(SaveAs, CanSave);
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
            var newRule = new Rule();
            newRule.Name = _nameEditorService.CreateNewName(
                _localization.GetLocalizedString("RulesPage.NewRulePrompt"),
                Rules.Select(f => f.Name).ToArray());
            Rules.Add(new RuleViewModel(newRule, _localization));
        } catch(OperationCanceledException) {
        }
    }

    private void RenameRule(RuleViewModel rule) {
        try {
            rule.Name = _nameEditorService.CreateNewName(
                _localization.GetLocalizedString("RulesPage.RenameRulePrompt"),
                Rules.Select(f => f.Name).ToArray(),
                rule.Name);
        } catch(OperationCanceledException) {
        }
    }

    private void CopyRule(RuleViewModel rule) {
        try {
            var copyRule = rule.GetRule().Copy();
            copyRule.Name = _nameEditorService.CreateNewName(
                _localization.GetLocalizedString("RulesPage.NewRulePrompt"),
                Rules.Select(f => f.Name).ToArray(),
                rule.Name);
            Rules.Add(new RuleViewModel(copyRule, _localization));
        } catch(OperationCanceledException) {
        }
    }

    private bool CanCopyRule(RuleViewModel rule) {
        return rule != null; // TODO валидация правила
    }

    private bool CanRenameRule(RuleViewModel rule) {
        return rule is not null;
    }

    private void RemoveRules(IList items) {
        var rules = items.OfType<RuleViewModel>().ToArray();
        foreach(var rule in rules) {
            Rules.Remove(rule);
        }
    }

    private bool CanRemoveRules(IList items) {
        return items != null
               && items.OfType<RuleViewModel>().Count() != 0;
    }

    private void Load() {
        // TODO
    }

    private void Save() {
        _rulesRepo.SetRules(Rules.Select(r => r.GetRule()).ToArray());
    }

    private void SaveAs() {
        // TODO
    }

    private bool CanSave() {
        return true; // TODO валидация
    }
}
