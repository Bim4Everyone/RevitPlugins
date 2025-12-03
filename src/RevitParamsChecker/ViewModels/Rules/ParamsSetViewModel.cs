using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitParamsChecker.Models.Rules;

namespace RevitParamsChecker.ViewModels.Rules;

internal class ParamsSetViewModel : BaseViewModel {
    private readonly LogicalRule _logicalRule;
    private readonly ICollection<ComparisonOperatorViewModel> _availableComparisonOperators;
    private LogicalOperatorViewModel _selectedOperator;

    public ParamsSetViewModel(
        LogicalRule logicalRule,
        ICollection<LogicalOperatorViewModel> logicalOperators,
        ICollection<ComparisonOperatorViewModel> comparisonOperators) {
        if(logicalOperators == null) {
            throw new ArgumentNullException(nameof(logicalOperators));
        }

        if(comparisonOperators == null) {
            throw new ArgumentNullException(nameof(comparisonOperators));
        }

        if(logicalOperators.Count == 0) {
            throw new ArgumentOutOfRangeException(nameof(logicalOperators));
        }

        if(comparisonOperators.Count == 0) {
            throw new ArgumentOutOfRangeException(nameof(comparisonOperators));
        }

        _logicalRule = logicalRule ?? throw new ArgumentNullException(nameof(logicalRule));
        _availableComparisonOperators = comparisonOperators;
        AvailableLogicalOperators = new ReadOnlyCollection<LogicalOperatorViewModel>(logicalOperators.ToArray());
        SelectedOperator = _logicalRule.Operator != null
            ? AvailableLogicalOperators.First(o => o.Operator.Equals(_logicalRule.Operator))
            : AvailableLogicalOperators.First();
        InnerParamRules = [
            .._logicalRule.ChildRules.OfType<ParameterRule>()
                .Select(r => new ParamRuleViewModel(r, _availableComparisonOperators))
        ];
        InnerParamSets = [
            .._logicalRule.ChildRules.OfType<LogicalRule>()
                .Select(r => new ParamsSetViewModel(r, AvailableLogicalOperators, _availableComparisonOperators))
        ];
        AddInnerParamRuleCommand = RelayCommand.Create(AddInnerRule);
        RemoveInnerParamRuleCommand = RelayCommand.Create<ParamRuleViewModel>(RemoveInnerRule, CanRemoveInnerRule);
        AddInnerParamSetCommand = RelayCommand.Create(AddInnerSet);
        RemoveInnerParamSetCommand = RelayCommand.Create<ParamsSetViewModel>(RemoveInnerSet, CanRemoveInnerSet);
    }

    public ICommand AddInnerParamRuleCommand { get; }
    public ICommand RemoveInnerParamRuleCommand { get; }
    public ICommand AddInnerParamSetCommand { get; }
    public ICommand RemoveInnerParamSetCommand { get; }

    public LogicalOperatorViewModel SelectedOperator {
        get => _selectedOperator;
        set => RaiseAndSetIfChanged(ref _selectedOperator, value);
    }

    public ICollection<LogicalOperatorViewModel> AvailableLogicalOperators { get; }

    public ObservableCollection<ParamRuleViewModel> InnerParamRules { get; }

    public ObservableCollection<ParamsSetViewModel> InnerParamSets { get; }

    public LogicalRule GetRule() {
        _logicalRule.Operator = SelectedOperator.Operator;
        _logicalRule.ChildRules = [
            ..InnerParamSets.Select(s => s.GetRule()), ..InnerParamRules.Select(s => s.GetRule())
        ];
        return _logicalRule;
    }

    private void AddInnerSet() {
        InnerParamSets.Add(
            new ParamsSetViewModel(new LogicalRule(), AvailableLogicalOperators, _availableComparisonOperators));
    }

    private void RemoveInnerSet(ParamsSetViewModel p) {
        InnerParamSets.Remove(p);
    }

    private bool CanRemoveInnerSet(ParamsSetViewModel p) {
        return p is not null;
    }

    private void AddInnerRule() {
        InnerParamRules.Add(new ParamRuleViewModel(new ParameterRule(), _availableComparisonOperators));
    }

    private void RemoveInnerRule(ParamRuleViewModel p) {
        InnerParamRules.Remove(p);
    }

    private bool CanRemoveInnerRule(ParamRuleViewModel p) {
        return p is not null;
    }
}
