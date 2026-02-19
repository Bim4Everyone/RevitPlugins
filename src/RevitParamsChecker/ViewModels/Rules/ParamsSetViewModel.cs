using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitParamsChecker.Models.Rules;
using RevitParamsChecker.Models.Rules.LogicalOperators;

namespace RevitParamsChecker.ViewModels.Rules;

internal class ParamsSetViewModel : BaseViewModel {
    private readonly LogicalRule _logicalRule;
    private readonly ILocalizationService _localization;
    private LogicalOperatorViewModel _selectedOperator;

    private static readonly IReadOnlyCollection<LogicalOperator> _availableLogicalOperators = [
        new AndOperator(), new OrOperator()
    ];

    public ParamsSetViewModel(
        LogicalRule logicalRule,
        ILocalizationService localization) {
        _logicalRule = logicalRule ?? throw new ArgumentNullException(nameof(logicalRule));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        AvailableLogicalOperators = new ReadOnlyCollection<LogicalOperatorViewModel>(
            _availableLogicalOperators
                .Select(o => new LogicalOperatorViewModel(_localization, o))
                .ToArray());
        SelectedOperator = _logicalRule.Operator != null
            ? AvailableLogicalOperators.First(o => o.Operator.Equals(_logicalRule.Operator))
            : AvailableLogicalOperators.First();
        InnerParamRules = [
            .._logicalRule.ChildRules.OfType<ParameterRule>()
                .Select(r => new ParamRuleViewModel(r, _localization))
        ];
        InnerParamSets = [
            .._logicalRule.ChildRules.OfType<LogicalRule>()
                .Select(r => new ParamsSetViewModel(r, _localization))
        ];
        AddInnerParamRuleCommand = RelayCommand.Create(AddInnerRule);
        RemoveInnerParamRuleCommand = RelayCommand.Create<ParamRuleViewModel>(RemoveInnerRule, CanRemoveInnerRule);
        AddInnerParamSetCommand = RelayCommand.Create(AddInnerSet);
        RemoveInnerParamSetCommand = RelayCommand.Create<ParamsSetViewModel>(RemoveInnerSet, CanRemoveInnerSet);
        SubscribeToPropertyChanged(InnerParamSets, InnerSetChanged);
        SubscribeToPropertyChanged(InnerParamRules, InnerRuleChanged);
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
        Validate();
        _logicalRule.Operator = SelectedOperator.Operator;
        _logicalRule.ChildRules = [
            ..InnerParamSets.Select(s => s.GetRule()), ..InnerParamRules.Select(s => s.GetRule())
        ];
        return _logicalRule;
    }

    public bool IsValid() {
        try {
            Validate();
            return true;
        } catch(InvalidOperationException) {
            return false;
        }
    }

    public void Validate() {
        if(SelectedOperator is null) {
            throw new InvalidOperationException($"Сначала надо назначить {nameof(SelectedOperator)}");
        }

        foreach(var rule in InnerParamRules) {
            rule.Validate();
        }

        foreach(var set in InnerParamSets) {
            set.Validate();
        }
    }

    private void AddInnerSet() {
        var vm = new ParamsSetViewModel(new LogicalRule(), _localization);
        InnerParamSets.Add(vm);
        vm.PropertyChanged += InnerSetChanged;
        OnPropertyChanged(nameof(InnerParamSets));
    }

    private void RemoveInnerSet(ParamsSetViewModel p) {
        InnerParamSets.Remove(p);
        p.PropertyChanged -= InnerSetChanged;
        OnPropertyChanged(nameof(InnerParamSets));
    }

    private bool CanRemoveInnerSet(ParamsSetViewModel p) {
        return p is not null;
    }

    private void AddInnerRule() {
        var vm = new ParamRuleViewModel(new ParameterRule(), _localization);
        InnerParamRules.Add(vm);
        vm.PropertyChanged += InnerRuleChanged;
        OnPropertyChanged(nameof(InnerParamRules));
    }

    private void RemoveInnerRule(ParamRuleViewModel p) {
        InnerParamRules.Remove(p);
        p.PropertyChanged -= InnerRuleChanged;
        OnPropertyChanged(nameof(InnerParamRules));
    }

    private bool CanRemoveInnerRule(ParamRuleViewModel p) {
        return p is not null;
    }

    private void InnerRuleChanged(object sender, PropertyChangedEventArgs args) {
        OnPropertyChanged(nameof(InnerParamRules));
    }

    private void InnerSetChanged(object sender, PropertyChangedEventArgs args) {
        OnPropertyChanged(nameof(InnerParamSets));
    }

    private void SubscribeToPropertyChanged(
        IEnumerable<INotifyPropertyChanged> items,
        PropertyChangedEventHandler handler) {
        foreach(var i in items) {
            i.PropertyChanged += handler;
        }
    }
}
