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
    private readonly ICollection<ComparisonOperatorViewModel> _availableComparisonOperators;
    private LogicalOperatorViewModel _selectedOperator;

    public ParamsSetViewModel(
        ICollection<LogicalOperatorViewModel> logicalOperators,
        ICollection<ComparisonOperatorViewModel> comparisonOperators) {
        if(logicalOperators == null) {
            throw new ArgumentNullException(nameof(logicalOperators));
        }

        if(logicalOperators.Count == 0) {
            throw new ArgumentOutOfRangeException(nameof(logicalOperators));
        }

        if(comparisonOperators.Count == 0) {
            throw new ArgumentOutOfRangeException(nameof(comparisonOperators));
        }

        _availableComparisonOperators =
            comparisonOperators ?? throw new ArgumentNullException(nameof(comparisonOperators));

        AvailableLogicalOperators = new ReadOnlyCollection<LogicalOperatorViewModel>(logicalOperators.ToArray());
        SelectedOperator = AvailableLogicalOperators.First();
        InnerParamRules = [];
        InnerParamSets = [];

        AddInnerParamRuleCommand = RelayCommand.Create(AddRule);
        RemoveInnerParamRuleCommand = RelayCommand.Create<ParamRuleViewModel>(RemoveRule, CanRemoveRule);
        AddInnerParamSetCommand = RelayCommand.Create(AddSet);
        RemoveInnerParamSetCommand = RelayCommand.Create<ParamsSetViewModel>(RemoveSet, CanRemoveSet);
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
        return new LogicalRule() {
            Operator = SelectedOperator.Operator,
            ChildRules = [..InnerParamSets.Select(s => s.GetRule()), ..InnerParamRules.Select(s => s.GetRule())]
        };
    }

    private void AddSet() {
        InnerParamSets.Add(new ParamsSetViewModel(AvailableLogicalOperators, _availableComparisonOperators));
    }

    private void RemoveSet(ParamsSetViewModel p) {
        InnerParamSets.Remove(p);
    }

    private bool CanRemoveSet(ParamsSetViewModel p) {
        return p is not null;
    }

    private void AddRule() {
        InnerParamRules.Add(new ParamRuleViewModel(_availableComparisonOperators));
    }

    private void RemoveRule(ParamRuleViewModel p) {
        InnerParamRules.Remove(p);
    }

    private bool CanRemoveRule(ParamRuleViewModel p) {
        return p is not null;
    }
}
