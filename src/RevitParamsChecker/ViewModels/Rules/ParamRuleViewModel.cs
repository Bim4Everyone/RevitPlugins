using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using dosymep.WPF.ViewModels;

using RevitParamsChecker.Models.Rules;

namespace RevitParamsChecker.ViewModels.Rules;

internal class ParamRuleViewModel : BaseViewModel {
    private string _paramName;
    private string _expectedValue;
    private ComparisonOperatorViewModel _selectedOperator;

    public ParamRuleViewModel(ICollection<ComparisonOperatorViewModel> operators) {
        if(operators == null) {
            throw new ArgumentNullException(nameof(operators));
        }

        if(operators.Count == 0) {
            throw new ArgumentException(nameof(operators));
        }

        AvailableOperators = new ReadOnlyCollection<ComparisonOperatorViewModel>(operators.ToArray());
        SelectedOperator = AvailableOperators.First();
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

        return new ParameterRule() {
            ParameterName = ParamName,
            ExpectedValue = ExpectedValue,
            Operator = SelectedOperator.Operator
        };
    }
}
