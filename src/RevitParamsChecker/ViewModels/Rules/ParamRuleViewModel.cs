using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitParamsChecker.Models.Rules;
using RevitParamsChecker.Models.Rules.ComparisonOperators;

namespace RevitParamsChecker.ViewModels.Rules;

internal class ParamRuleViewModel : BaseViewModel {
    private readonly ParameterRule _rule;
    private readonly ILocalizationService _localization;
    private string _paramName;
    private string _expectedValue;
    private ComparisonOperatorViewModel _selectedOperator;

    private static readonly IReadOnlyCollection<ComparisonOperator> _availableComparisonOperators = [
        new EqualsOperator(),
        new NotEqualsOperator(),
        new GreaterOperator(),
        new GreaterOrEqualOperator(),
        new LessOperator(),
        new LessOrEqualOperator(),
        new BeginsWithOperator(),
        new NotBeginsWithOperator(),
        new EndsWithOperator(),
        new NotEndsWithOperator(),
        new ContainsOperator(),
        new NotContainsOperator(),
        new HasValueOperator(),
        new HasNoValueOperator()
    ];

    public ParamRuleViewModel(ParameterRule rule, ILocalizationService localization) {
        _rule = rule ?? throw new ArgumentNullException(nameof(rule));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));

        AvailableOperators = new ReadOnlyCollection<ComparisonOperatorViewModel>(
            _availableComparisonOperators
                .Select(o => new ComparisonOperatorViewModel(_localization, o))
                .ToArray());
        SelectedOperator = _rule.Operator != null
            ? AvailableOperators.First(o => o.Operator.Equals(_rule.Operator))
            : AvailableOperators.First();
        ParamName = _rule.ParameterName;
        ExpectedValue = _rule.ExpectedValue;
    }

    public string ParamName {
        get => _paramName;
        set => RaiseAndSetIfChanged(ref _paramName, value);
    }

    public ComparisonOperatorViewModel SelectedOperator {
        get => _selectedOperator;
        set => RaiseAndSetIfChanged(ref _selectedOperator, value);
    }

    public IReadOnlyCollection<ComparisonOperatorViewModel> AvailableOperators { get; }

    public string ExpectedValue {
        get => _expectedValue;
        set => RaiseAndSetIfChanged(ref _expectedValue, value);
    }

    public ParameterRule GetRule() {
        if(string.IsNullOrWhiteSpace(ParamName)) {
            throw new InvalidOperationException($"Сначала надо назначить {nameof(ParamName)}");
        }

        if(string.IsNullOrWhiteSpace(ExpectedValue)) {
            throw new InvalidOperationException($"Сначала надо назначить {nameof(ExpectedValue)}");
        }

        if(SelectedOperator is null) {
            throw new InvalidOperationException($"Сначала надо назначить {nameof(SelectedOperator)}");
        }

        _rule.ParameterName = ParamName;
        _rule.ExpectedValue = ExpectedValue;
        _rule.Operator = SelectedOperator.Operator;
        return _rule;
    }
}
